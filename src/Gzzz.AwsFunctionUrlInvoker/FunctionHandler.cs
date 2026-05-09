using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Runtime.Internal;
using Gzzz;
using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker.Models;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Gzzz.CommandInvoker;
using Gzzz.Db.DynamoDb;
using Gzzz.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker;

public class FunctionHandler
{

	public readonly IServiceProvider Services;
	readonly IReadOnlyDictionary<string, CommandInfo> _commands;
	readonly TimeService _timeService;
	readonly IContextSerializer _contextSerializer;
	readonly ITextLogger _logger;
	readonly TokenService _tokenService;

	bool _isColdStart = true;
	public FunctionHandler(Assembly[] assemblies, Action<IServiceCollection> configureServices)
	{
		IServiceCollection serviceCollection= new ServiceCollection()
			.AddSingleton<ITextLogger, JsonLogger>()
			.AddSingleton<IContextSerializer, JsonContextSerializer>()
			.AddSingleton<TimeService>()
			.AddScoped<ApiContext>()
			.AddScoped<RequestInfo>(services=> (RequestInfo)services.GetRequiredService<ApiContext>())
			//
			.AddSingleton<IUserRepository, DefaultUserRepository>()
			.AddSingleton<IUserAuthenticatedInfoUpdater, DefaultTokenUpdateService>()
			
			.AddSingleton<TokenService>()
			.AddSingleton<InternalIpService>()
			.AddEnvironmentObject<TokenServiceConfig>(TokenServiceConfig.EnvironmentVariableName)
			//
			.AddCommandInvokers(assemblies)
		;


		configureServices(serviceCollection);

		this.Services = serviceCollection.BuildWithValidation();
		_commands = Services.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>();
		_timeService = Services.GetRequiredService<TimeService>();
		_tokenService = Services.GetRequiredService<TokenService>();
		_contextSerializer = Services.GetRequiredService<IContextSerializer>();
		_logger = Services.GetRequiredService<ITextLogger>();
	}


	public Task RunAsync() => LambdaBootstrapBuilder
		.Create<FunctionUrlRequest, FunctionUrlResponse>(RequestHandleAsync, new SourceGeneratorLambdaJsonSerializer<FunctionUrlJsonContext>())
		.Build()
		.RunAsync();

	public async Task<FunctionUrlResponse> RequestHandleAsync(FunctionUrlRequest request)
	{

		using var scope = Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
		if (_isColdStart)
		{
			context.IsColdStart = true;
			_isColdStart = false;
		}
		context.Ip = request.RequestContext.Http.SourceIp;
		context.RequestTime = _timeService.GetNow();
		context.API = request.RequestContext.Http.Path;

		FunctionUrlResponse response = await HandleAsync(scope.ServiceProvider, request, context);

		context.Status = response.StatusCode;
		context.Duration = (int)(_timeService.GetNow() - context.RequestTime).TotalMilliseconds;
		//threshold 적용해서 일정수치 이하면 아예 기록하지 않는것도 고려해볼만함

		if (context.SkipLogging == false)
			_logger.WriteObject(context);
		return response;
	}

	public async Task<FunctionUrlResponse> HandleAsync(IServiceProvider services, FunctionUrlRequest request, ApiContext context)
	{
		if (_commands.TryGetValue(request.RequestContext.Http.Path, out var command) == false)
		{
			context.RequestPath = context.API;
			context.API = null;
			return ResponsePreset.NotFound;//로그를 남길까 말까~
		}
										   //
		TokenClaims claims = default; //어차피 stackalloc이 안되니깐 그냥 struct로 만들어서 씀
		if (command.IsAuthenticationRequired)
		{
			var decodeTokenResult = _tokenService.ValidateToken(TokenType.AccessTokenV1, request.Headers.AccessToken, context, out claims);
			if (decodeTokenResult.IsSuccess == false)
				return FunctionUrlResponseHelper.Error(401, (int)decodeTokenResult.ErrorCode);
		}
		try
		{
			if (command.IsParameterRequired)
			{
				var requestBody = request.GetRequestBody();
				context.RequestRaw = requestBody;
				context.RequestModel = _contextSerializer.Derialize(requestBody, command.RequestType);
				context.RequestRaw = null;
			}

			//db 연산을 하기 때문에 최후 순위로 미룸
			if (command.IsAuthenticationRequired
				&& await services.GetRequiredService<IUserRepository>().LoadAsync(claims.UserId, claims.CreatedAt) == false) 
			{
				return ResponsePreset. DiscardedAuthentication;
			}

			context.ResponseModel = await command.InvokeAsync(services, context.RequestModel);
			string deserializedResponseBody = command.ResponseType != null ?
				_contextSerializer.Serialize(context.ResponseModel, command.ResponseType)
				: null;
			context.TrimSuccess(command.LoggingType);
			return FunctionUrlResponseHelper.Success(deserializedResponseBody);
		}
		catch (HttpException httpException)
		{
			context.ErrorCode = httpException.ErrorCode;
			context.ErrorMessage = httpException.Message;
			return FunctionUrlResponseHelper.Error(httpException.StatusCode, httpException.Message, httpException.Message, httpException.ErrorCode);
		}
		catch (JsonException)
		{
			return ResponsePreset.DeserializeFail;
		}
		catch (Exception ex)
		{
			context.Exception = ex.ToString();
			return FunctionUrlResponseHelper.Error(500, "Unexpected Exception", ex.ToString(), 0); //release 전에는 감춰야함
		}
	}
}


public static class ResponsePreset
{
	public static readonly FunctionUrlResponse NotFound = FunctionUrlResponseHelper.Error(404);
	public static readonly FunctionUrlResponse DeserializeFail = FunctionUrlResponseHelper.Error(400);
	public static readonly FunctionUrlResponse DiscardedAuthentication = FunctionUrlResponseHelper.Error(401, (int)UnauthorizedErrorCode.DiscardedAuthentication);
}

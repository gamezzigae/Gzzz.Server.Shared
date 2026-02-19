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
			.AddSingleton<IUserAuthenciatedInfoUpdater, DefaultTokenUpdateService>()
			
			.AddSingleton<TokenService>()
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
		context.Path = request.RequestContext.Http.Path;

		FunctionUrlResponse response = await HandleAsync(scope.ServiceProvider, request, context);

		context.Status = response.StatusCode;
		context.Elapsed = (int)(_timeService.GetNow() - context.RequestTime).TotalMilliseconds;

		if (context.SkipLogging == false)
			_logger.WriteObject(context);
		return response;
	}

	public async Task<FunctionUrlResponse> HandleAsync(IServiceProvider services, FunctionUrlRequest request, ApiContext context)
	{
		if (_commands.TryGetValue(request.RequestContext.Http.Path, out var command) == false)
			return ResponsePreset.NotFound;//로그를 남길까 말까~
										   //
		if (command.IsAuthenticationRequired)
		{
			var decodeTokenResult = _tokenService.ValidateToken(TokenType.AccessTokenV1, request.Headers.AccessToken, context, out var claims);
			if (decodeTokenResult.IsSuccess == false)
				return FunctionUrlResponseHelper.Error(401, decodeTokenResult.ErrorMessage, null, 0);

			var userRepository = services.GetRequiredService<IUserRepository>();
			await userRepository.LoadAsync(claims);
		}
		//
		if (command.IsParameterRequired)
		{
			var requestBody = request.GetRequestBody();
			try
			{
				context.RequestModel = _contextSerializer.Derialize(requestBody, command.RequestType);
			}
			catch (Exception)
			{
				context.RequestRaw = requestBody;
				return ResponsePreset.DeserializeFail;
			}
		}
		//
		try
		{
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


}

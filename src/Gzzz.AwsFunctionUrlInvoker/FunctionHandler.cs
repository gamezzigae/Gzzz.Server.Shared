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
	readonly JsonLogger _logger;
	readonly AuthenticationService _authenticationService;

	bool _isColdStart = true;
	public FunctionHandler(Assembly[] assemblies, JsonSerializerOptions jsonSerializerOptions, Action<IServiceCollection> configureServices)
	{
		IServiceCollection serviceCollection= new ServiceCollection()
			.AddSingleton<IAccountScopedRepository, DefaultAccountScopedRepository>()
			.AddSingleton<JsonLogger>()
			.AddSingleton<IContextSerializer, JsonContextSerializer>()
			.AddSingleton<TimeService>()
			.AddScoped<ApiContext>()
			//
			.AddSingleton<AuthenticationService>()
			.AddSingleton<TokenService>()
			.AddEnvironmentObject<AuthenticationConfig>("ZZ_AUTHENTICATION_CONFIG", jsonSerializerOptions)
			//
			.AddCommandInvokers(assemblies)
		;

		if (jsonSerializerOptions == null)
			serviceCollection.AddSingleton<JsonSerializerOptions>(_ => null);
		else
			serviceCollection.AddSingleton(jsonSerializerOptions);


		configureServices(serviceCollection);
	
		this.Services = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
		_commands = Services.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>();
		_timeService = Services.GetRequiredService<TimeService>();
		_contextSerializer = Services.GetRequiredService<IContextSerializer>();
		_logger = Services.GetRequiredService<JsonLogger>();
		_authenticationService = Services.GetRequiredService<AuthenticationService>();
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
			_logger.Write(context);

		response.Headers.Add("elased", context.Elapsed.ToString());
		response.Headers.Add("cold_start", context.IsColdStart.ToString());
		return response;
	}

	public async Task<FunctionUrlResponse> HandleAsync(IServiceProvider services, FunctionUrlRequest request, ApiContext context)
	{
		if (_commands.TryGetValue(request.RequestContext.Http.Path, out var command) == false)
			return ResponsePreset.NotFound;//로그를 남길까 말까~
										   //
		if (command.IsAuthenticationRequired)
		{
			var authenticationResult = await _authenticationService.ValidateTokenAsync(TokenType.Access, request.Headers.AccessToken, context);
			if (authenticationResult.IsSuccess == false)
				return FunctionUrlResponseHelper.Error(401, authenticationResult.ErrorMessage, null, 0);
		}
		//
		if (command.IsParameterRequired)
		{
			var requestBody = request.GetRequestBody();
			try
			{
				context.RequestModel = _contextSerializer.Derialize(command.RequestType, requestBody);
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
			string deserializedResponseBody = _contextSerializer.Serialize(context.ResponseModel); //예외처리 하지 않는다.
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

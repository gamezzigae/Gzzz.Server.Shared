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
using System.Reflection.Metadata;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker;

[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
public class FunctionHandler(IServiceProvider services)
{
	readonly ILambdaSerializer _lambdaSerializer = new SourceGeneratorLambdaJsonSerializer<FunctionUrlJsonContext>();
	readonly IServiceProvider _services = services;
	readonly IReadOnlyDictionary<string, CommandInfo> _commands = services.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>();
	readonly TimeService _timeService = services.GetRequiredService<TimeService>();
	readonly IContextSerializer _contextSerializer = services.GetRequiredService<IContextSerializer>();
	readonly JsonLogger _logger = services.GetRequiredService<JsonLogger>();
	readonly AuthenticationService _authenticationService = services.GetRequiredService<AuthenticationService>();

	bool _isColdStart = true;

	public Task RunAsync()=>LambdaBootstrapBuilder
		.Create<FunctionUrlRequest, FunctionUrlResponse>(RequestHandleAsync, _lambdaSerializer)
		.Build()
		.RunAsync();

	public async Task<FunctionUrlResponse> RequestHandleAsync(FunctionUrlRequest request)
	{
		using var scope = _services.CreateScope();
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
		//
		if (_commands.TryGetValue(request.RequestContext.Http.Path, out var command) == false)
			return FunctionUrlResponseHelper.Error(404, "command not found", 0);
		//
		if (command.AuthenticationRequired)
		{
			var accountScopedRepository = services.GetRequiredService<IAccountScopedRepository>();
			var authenticationResult = await _authenticationService.ValidateTokenAsync(TokenType.Access, request.Headers.AccessToken, context, accountScopedRepository);
			if (authenticationResult.IsSuccess == false)
			{
				return FunctionUrlResponseHelper.Error(401, authenticationResult.ErrorMessage, 0);
			}
		}
		//
		if (command.ParameterRequired)
		{
			try
			{
				context.RequestModel = _contextSerializer.Derialize(command.RequestType, request.GetRequestBody());
			}
			catch (Exception)
			{
				return FunctionUrlResponseHelper.Error(400, "request deserialize failed", 0);
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
			return FunctionUrlResponseHelper.Error(httpException.StatusCode, httpException.Message, httpException.ErrorCode);
		}
		catch (Exception ex)
		{
			context.Exception = ex.ToString();
			return FunctionUrlResponseHelper.Error(500, ex.ToString(), 0); //release 전에는 감춰야함
		}
	}
	
}

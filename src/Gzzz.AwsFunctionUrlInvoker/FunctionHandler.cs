using Amazon.Lambda.RuntimeSupport;
using Gzzz.AwsFunctionUrlInvoker.Models;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Gzzz.CommandInvoker;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker;

[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
public class FunctionHandler(IServiceProvider services)
{
	 
	readonly IServiceProvider _services = services;
	readonly IReadOnlyDictionary<string, CommandInfo> _commands = services.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>();
	readonly TimeService _timeService = services.GetRequiredService<TimeService>();
	readonly IContextSerializer _contextSerializer = services.GetRequiredService<IContextSerializer>();
	readonly JsonLogger _logger = services.GetRequiredService<JsonLogger>();
	bool _isColdStart = true;


	public Task RunAsync() => LambdaBootstrapBuilder.Create(this.StreamHandleAsync).Build().RunAsync();

	public async Task<Stream> StreamHandleAsync(Stream requestStream)
	{
		var request = await JsonSerializer.DeserializeAsync(requestStream, FunctionUrlJsonContext.Default.FunctionUrlRequest);
		using var scope = _services.CreateScope();

		var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
		context.IsColdStart = _isColdStart;
		_isColdStart = false;
		context.Ip = request.RequestContext.Http.SourceIp;
		context.RequestTime = _timeService.GetNow();
		context.Path = request.RequestContext.Http.Path;

		FunctionUrlResponse response;
		try
		{
			response = await HandleAsync(scope.ServiceProvider, request, context);
		}
		catch (HttpException httpException)
		{
			response = FunctionUrlResponseHelper.Error(httpException.StatusCode, httpException.Message, httpException.ErrorCode);
			context.ErrorCode = httpException.ErrorCode;
			context.ErrorMessage = httpException.Message;
		}
		catch (Exception ex)
		{
			response = FunctionUrlResponseHelper.Error(500, ex.ToString(), 0); //release 전에는 감춰야함
			context.Exception = ex.ToString();
		}

		context.Status = response.StatusCode;
		context.Elapsed = (int)(_timeService.GetNow() - context.RequestTime).TotalMicroseconds;

		_logger.Write(context);

		var responseStream = new MemoryStream();
		await JsonSerializer.SerializeAsync(responseStream, response, FunctionUrlJsonContext.Default.FunctionUrlResponse);
		responseStream.Position = 0;
		return responseStream;
	}

	Task<string> GetUserIdAsync(string accessToken)
	{
		return Task.FromResult(accessToken);
	}

	

	public async Task<FunctionUrlResponse> HandleAsync(IServiceProvider services, FunctionUrlRequest request, ApiContext context)
	{
		//
		if (_commands.TryGetValue(request.RequestContext.Http.Path, out var command) == false)
			return FunctionUrlResponseHelper.Error(404, "command not found", 0);
		//
		if (command.AuthenticationRequired)
		{
			try
			{
				context.UserId = await GetUserIdAsync(request.Headers.AccessSignature);
			}
			catch (Exception)
			{
				return FunctionUrlResponseHelper.Error(401, "authentication required", 0);
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
		context.ResponseModel = await command.InvokeAsync(services, context.RequestModel);
		string deserializedResponseBody = _contextSerializer.Serialize(context.ResponseModel); //예외처리 하지 않는다.
																							   //
		return FunctionUrlResponseHelper.Success(deserializedResponseBody);
	}
	
}

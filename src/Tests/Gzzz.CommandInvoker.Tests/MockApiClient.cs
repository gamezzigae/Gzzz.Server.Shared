using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Models;
using System.Text.Json;

namespace Gzzz.CommandInvoker.Tests;

public class MockTimeService : TimeService
{
	DateTime? _now;
	
	public override DateTime GetNow()
	{
		if (_now.IsNotDefault())
			return _now.Value;
		return DateTime.UtcNow;
	}
	public DateTime SetNow(DateTime time)
	{
		_now = time;
		return time;
	}
}

public class MockApiClient : IApiClient	
{
	readonly FunctionHandler _functionHandler;
	public string Ip { get; set; }

	public AuthenticationTokens AuthenticationTokens { get; set; }

	public int RequestId { get; set; }
	public MockApiClient(FunctionHandler functionHandler)
	{
		this._functionHandler = functionHandler;
		this.Ip = "128.1.2.3";
	}

	public async Task<T> RequestAsync<T>(string path, ApiOption apiOption, object requestBody= null)
	{
		IApiClient client = this;
		var request = new FunctionUrlRequest()
		{
			RawPath = path,
			Headers = new FunctionUrlHeaders()
			{
				AccessToken = apiOption == ApiOption.Anonymous ? default : client.GetAccessToken(),
				RequestId = apiOption == ApiOption.Idempotency ? RequestId.ToString() : default
			},
			RequestContext = new RequestContext()
			{
				Http = new FunctionUrlHttpContext() { Path = path, SourceIp = Ip },
			},
			Body = requestBody != null ? JsonSerializer.Serialize(requestBody) : null
		};

		var response = await _functionHandler.RequestHandleAsync(request);

		if(response.StatusCode >201)
		{
			var errorMessage = response.Headers.GetValueOrDefault("zz-em");
			int.TryParse(response.Headers.GetValueOrDefault("zz-ec"), out var errorCode);
			throw new HttpException(response.StatusCode, errorMessage, errorCode);
		}

		var result = JsonSerializer.Deserialize<T>(response.Body);
		return result;
	}

}

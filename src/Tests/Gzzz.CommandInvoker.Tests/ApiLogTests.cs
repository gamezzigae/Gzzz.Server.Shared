using System.Text.Json;

namespace Gzzz.CommandInvoker.Tests;

public class ApiLogTests : AwsFunctionUrlInvokerFixture
{
	readonly IApiClient _client;
	protected MockApiClient _mockApiClient => (MockApiClient)_client;
	public ApiLogTests(ITestOutputHelper testLogger) : base(testLogger)
	{
		_client = CreateEmptyClient();
	}

	[Fact]
	public async Task ApiLogTest()
	{
		var now = _mockTimeService.SetNow();
		var path = "/test/hello";
		var expectedResponse = "world";
		var response = await _client.RequestAsync<string>(path, ApiOption.Anonymous);
		Assert.Equal(expectedResponse, response);

		var log = _mockJsonLogger.DequeueApiLog();

		Assert.Equal(path, log.Path);
		Assert.Equal(200, log.Status);
		Assert.Equal(now, log.RequestTime);
		Assert.Equal(expectedResponse, ((JsonElement)log.ResponseModel).GetString());
	}

	[Fact]
	public async Task RequestInfoTest()
	{
		var now = base._mockTimeService.SetNow();
		var path = "/me/__requestinfo__";
		var response = await _client.RequestAsync<RequestInfo>(path, ApiOption.Anonymous);

		Assert.Equal(now, response.RequestTime);
		Assert.Equal(_mockApiClient.Ip, response.Ip);
	}

	[Fact]
	public async Task NotFoundErrorTestAsync()
	{
		var path = "/not/exist/path";
		var exception = await Assert.ThrowsAsync<HttpException>(async () =>
		{
			await _client.RequestAsync<object>(path, ApiOption.Anonymous);
		});
		Assert.Equal(404, exception.StatusCode);
		Assert.Equal(0, exception.ErrorCode);

		var log = _mockJsonLogger.DequeueApiLog();
		Assert.Equal(path, log.Path);
		Assert.Equal(404, log.Status);
	}

	[Fact]
	public async Task BodyDeserializeTestAsync()
	{
		var now = DateTimeOffset.UtcNow;
		var echo = await _client.RequestAsync<DateTimeOffset>("/test/echodatetime", ApiOption.Anonymous, now.ToString("O")); ;

		Assert.Equal(now, echo);
	}

	[Fact]
	public async Task BodyDeserializeFailTestAsync()
	{
		var exception = await Assert.ThrowsAsync<HttpException>(async () =>
		{
			await _client.RequestAsync<DateTime>("/test/echodatetime", ApiOption.Anonymous, "abc");
		});
		Assert.Equal(400, exception.StatusCode);
		Assert.Equal(0, exception.ErrorCode);

		var log = _mockJsonLogger.DequeueApiLog();

		Assert.Equal("\"abc\"", log.RequestRaw);
	}
}

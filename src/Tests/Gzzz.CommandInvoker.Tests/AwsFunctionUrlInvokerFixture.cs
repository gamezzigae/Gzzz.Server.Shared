using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Gzzz.CommandInvoker.Tests;

public class AwsFunctionUrlInvokerFixture
{
	protected readonly FunctionHandler _functionHandler;
	protected readonly MockJsonLogger _mockJsonLogger;
	protected readonly MockTimeService _mockTimeService;
	readonly ITestOutputHelper _testLogger;

	public AwsFunctionUrlInvokerFixture(ITestOutputHelper testLogger)
	{
		EnvironmentX.SetObject(AuthenticationConfig.EnvironmentVariableName, new AuthenticationConfig()
		{
			HashKey = "abc12312",
			AccessTokenLIfetime = 1000,
			RefreshTokenLifetime = 3000,
		});

		_functionHandler = new FunctionHandler(
			[typeof(TestController).Assembly, typeof(Gzzz.Controllers.SignController).Assembly],
			default,
			services => services
				.AddSingleton<TimeService, MockTimeService>()
				.AddSingleton<JsonLogger, MockJsonLogger>()
			);

		_mockJsonLogger = GetRequiredService<JsonLogger, MockJsonLogger>();
		_mockTimeService = GetRequiredService<TimeService, MockTimeService>();
		_testLogger = testLogger;
	}

	public IApiClient CreateEmptyClient() => new MockApiClient(this._functionHandler, _testLogger);

	public async Task<IApiClient> CreateSignedClientAsync(string userId)
	{
		var client = CreateEmptyClient();
		var tokens = await client.RequestAsync<AuthenticationTokens>("/sign/_____impersonate_____", ApiOption.Anonymous, userId);
		client.AuthenticationTokens = tokens;
		return client;
	}

	public T GetService<T>() => _functionHandler.Services.GetRequiredService<T>();
	public TImplementation GetRequiredService<TService, TImplementation>() where TImplementation : TService => (TImplementation)GetService<TService>();
}

public class ApiErrorTests : AwsFunctionUrlInvokerFixture
{
	readonly IApiClient _client;
	public ApiErrorTests(ITestOutputHelper testLogger) : base(testLogger)
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

public class MockJsonLogger : JsonLogger
{
	public MockJsonLogger() : base(JsonSerializerOptions.Default)
	{
	}

	public readonly Queue<string> Queue = new Queue<string>();

	public override void Write(string message)
	{
		Queue.Enqueue(message);
	}

	public ApiContext DequeueApiLog()
	{
		var log = Queue.Dequeue();
		return JsonSerializer.Deserialize<ApiContext>(log)!;
	}
}

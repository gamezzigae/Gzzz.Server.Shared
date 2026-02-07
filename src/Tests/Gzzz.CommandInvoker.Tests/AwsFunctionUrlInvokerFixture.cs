using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.CommandInvoker.Tests;

public class AwsFunctionUrlInvokerFixture
{
	readonly FunctionHandler _functionHandler;

	public AwsFunctionUrlInvokerFixture()
	{
		EnvironmentX.SetObject("ZZ_AUTHENTICATION_CONFIG", new AuthenticationConfig()
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
			);
	}

	public IApiClient CreateEmptyClient() => new MockApiClient(this._functionHandler);

	public async Task<IApiClient> CreateSignedClientAsync(string userId)
	{
		var client = CreateEmptyClient();
		var tokens = await client.RequestAsync<AuthenticationTokens>("/sign/_____impersonate_____", ApiOption.Anonymous, userId);
		client.AuthenticationTokens = tokens;
		return client;
	}

	public T GetService<T>() => _functionHandler.Services.GetRequiredService<T>();

}

public class ApiErrorTests : AwsFunctionUrlInvokerFixture
{
	readonly IApiClient _client;
	public ApiErrorTests()
	{
		_client = CreateEmptyClient();
	}
	[Fact]
	public async Task NotFoundErrorTestAsync()
	{
		var exception = await Assert.ThrowsAsync<HttpException>(async () =>
		{
			await _client.RequestAsync<object>("/not/exist/path", ApiOption.Anonymous);
		});
		Assert.Equal(404, exception.StatusCode);
		Assert.Equal(0, exception.ErrorCode);
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
	}
}

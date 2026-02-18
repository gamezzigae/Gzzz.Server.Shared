using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Gzzz.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.AwsFunctionUrlInvoker.Test;

public abstract class AwsFunctionUrlInvokerFixtureBase
{
	protected readonly FunctionHandler _functionHandler;
	protected readonly MockJsonLogger _mockJsonLogger;
	protected readonly MockTimeService _mockTimeService;
	protected readonly ITestOutputHelper _testLogger;

	protected abstract Assembly[] GetAssemblies();

	protected abstract void ConfigureServices(IServiceCollection services);

	public AwsFunctionUrlInvokerFixtureBase(ITestOutputHelper testLogger)
	{
		EnvironmentX.SetObject(AuthenticationConfig.EnvironmentVariableName, new AuthenticationConfig()
		{
			HashKey = "abc12312",
			AccessTokenLIfetime = 1000,
			RefreshTokenLifetime = 3000,
		});

		_functionHandler = new FunctionHandler(
			GetAssemblies(),
			services => {
				services
				.AddSingleton<TimeService, MockTimeService>()
				.AddSingleton<ITextLogger, MockJsonLogger>();

				ConfigureServices(services);
			});

		_mockJsonLogger = GetRequiredService<ITextLogger, MockJsonLogger>();
		_mockTimeService = GetRequiredService<TimeService, MockTimeService>();
		_testLogger = testLogger;
	}

	public IApiClient CreateEmptyClient() => new MockApiClient(this._functionHandler, _testLogger);

	public async Task<IApiClient> CreateSignedClientAsync(string userId)
	{
		var client = CreateEmptyClient();
		var tokens = await client.RequestAsync<AuthenticationTokens>("/s/_____impersonate_____", ApiOption.Anonymous, userId);
		client.AuthenticationTokens = tokens;
		return client;
	}

	public T GetService<T>() => _functionHandler.Services.GetRequiredService<T>();
	public TImplementation GetRequiredService<TService, TImplementation>() where TImplementation : TService => (TImplementation)GetService<TService>();
}

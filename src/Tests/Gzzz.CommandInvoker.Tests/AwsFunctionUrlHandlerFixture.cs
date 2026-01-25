using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.CommandInvoker.Tests;

public class AwsFunctionUrlHandlerFixture
{
	readonly FunctionHandler _functionHandler;
	static readonly AuthenticationConfig _authenticationConfig = new AuthenticationConfig()
	{
		HashKey = "abc12312",
		AccessTokenLIfetime=1000,
		RefreshTokenLifetime=3000,
	};

	public AwsFunctionUrlHandlerFixture()
	{
		Assembly[] assemblies = [typeof(TestController).Assembly, typeof(Gzzz.Controllers.SignController).Assembly];
		_functionHandler = new FunctionHandlerBuilder(default)
			.UseCommandInvokers(assemblies)
			.UseAuthentication<AuthenticationService>(_authenticationConfig)
			.ConfigureServices(services => services
				.AddSingleton<IContextSerializer, JsonContextSerializer>()
				.AddSingleton<TimeService, MockTimeService>()
			).BuildFunctionHandler();
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

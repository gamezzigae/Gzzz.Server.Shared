using Amazon.Runtime;
using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Gzzz.Client;
using Gzzz.Db.DynamoDb;
using Gzzz.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.AwsFunctionUrlInvoker.Test;

[Collection("Sequential")]
public abstract class AwsFunctionUrlInvokerFixtureBase : IAsyncLifetime
{
	protected readonly FunctionHandler _functionHandler;
	protected readonly MockJsonLogger _mockJsonLogger;
	protected readonly MockTimeService _mockTimeService;
	protected readonly ITestOutputHelper _testLogger;

	protected abstract Assembly[] GetAssemblies();

	protected abstract void ConfigureServices(IServiceCollection services);

	public static readonly string InternalIp = "unit.test.i.p";
	public AwsFunctionUrlInvokerFixtureBase(ITestOutputHelper testLogger)
	{
		EnvironmentX.SetObject(DynamoDbConfig.EnvironmentVariableName,
			new DynamoDbConfig() { TableName = RandomX.GetRandomText(255), ServiceURL = "http://localhost:8000" });

		EnvironmentX.SetObject(TokenServiceConfig.EnvironmentVariableName, new TokenServiceConfig()
		{
			HashKey = "abc12312",
			AccessTokenLifetime = TimeSpan.FromSeconds(10),
			RefreshTokenLifetime = TimeSpan.FromSeconds(255),
		});
		EnvironmentX.SetValue(InternalIpService.EnvironmentVariableName, InternalIp);

		_functionHandler = new FunctionHandler(
			GetAssemblies(),
			services => {
				services
				.AddSingleton<AWSCredentials>(new BasicAWSCredentials("DUMMYACCESSKEYDUMMYY", "44nPdvh6gW+EXjh1P6jLXFzmmp4K2F1dUSQx7R4+"))
				.AddDynamoDbService().AddScoped<IUserRepository, DynamoDbUserRepositoryBase>()

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

	public async ValueTask InitializeAsync()
	{
		await DynamoDbTestUtil.CreateTableAsync(GetService<DynamoDbService>(), GetService<DynamoDbConfig>());
	}
	public async ValueTask DisposeAsync()
	{
		await DynamoDbTestUtil.DeleteTableAsync(GetService<DynamoDbService>(), GetService<DynamoDbConfig>());
	}
}

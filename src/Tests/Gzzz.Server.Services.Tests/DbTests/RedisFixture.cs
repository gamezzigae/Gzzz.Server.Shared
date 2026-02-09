using Gzzz.Db.Redis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class RedisFixture : IAsyncLifetime
{
	protected readonly RedisOptimisticRepository<TestEntity> _repository;
	readonly RedisConfig _redisConfig;
	public RedisFixture()
	{
		var services = ConfigureServices(new ServiceCollection()).BuildWithValidation();
		_redisConfig = services.GetRequiredService<RedisConfig>();
		_repository = new RedisOptimisticRepository<TestEntity>(services.GetRequiredService<RedisService>(), RandomX.GetRandomText());
	}

	public static IServiceCollection ConfigureServices(IServiceCollection services)
		=> services.AddSingleton<RedisConfig>(new RedisConfig() { Endpoint = "127.0.0.1,defaultDatabase=1,allowAdmin=true" })
			.AddSingleton<RedisService>()
			.AddSingleton<RedisOptimisticRepository<TestEntity>>()
			.AddSingleton<string>(RandomX.GetRandomText()); //redis prefix partition key, 테스트용이니까 그냥 씀

	public async ValueTask InitializeAsync() => await FlushAsync();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	public async Task FlushAsync()
	{
		var connectionMultiplexer = ConnectionMultiplexer.Connect(_redisConfig.Endpoint);
		var endpoint = connectionMultiplexer.GetEndPoints()[0];
		await connectionMultiplexer.GetServer(endpoint).FlushDatabaseAsync();
	}
}

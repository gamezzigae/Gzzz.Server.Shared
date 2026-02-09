using Gzzz.Db.Redis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class TestRedisRepository : RedisOptimisticRepository<TestEntity>
{
	static readonly RedisConfig _redisConfig = new RedisConfig() { Endpoint = "127.0.0.1,defaultDatabase=1,allowAdmin=true" };
	public TestRedisRepository() : base(new RedisService(_redisConfig), RandomX.GetRandomText())
	{
	}



	public async Task FlushAsync()
	{
		var connectionMultiplexer = ConnectionMultiplexer.Connect(_redisConfig.Endpoint);
		var endpoint = connectionMultiplexer.GetEndPoints()[0];
		await connectionMultiplexer.GetServer(endpoint).FlushDatabaseAsync();
	}
}

public class RedisFixture : IAsyncLifetime
{
	protected readonly RedisOptimisticRepository<TestEntity> _repository;
	readonly RedisConfig _redisConfig;
	public RedisFixture()
	{
		var services = ConfigureServices(new ServiceCollection()).ValidatedBuild();
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

public class RedisRepositoryTests : RedisFixture
{
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

	[Fact]
	public async Task InsertItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		await _repository.PutItemAsync(item.UserId, item, _now);
		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		Assert.NotEqual(default, retrievedItem);
		AssertX.JsonEquals(item, retrievedItem.Value);
		Assert.True(retrievedItem.IsFromCache);
		Assert.Equal(retrievedItem.UpdatedAt.ToUnixTimeMilliseconds(), (_now.ToUnixTimeMilliseconds()));
	}

	[Fact]
	public async Task InsertDuplicatedItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTimeOffset.UtcNow;
		await _repository.PutItemAsync(item.UserId, item, now);
		var exception = Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now));
	}

	[Fact]
	public async Task UpdateItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		await _repository.PutItemAsync(item.UserId, item, _now);

		var nextLevel = RandomX.GetRandom();
		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		item.Level = nextLevel;
		await _repository.PutItemAsync(item.UserId, item, _now.AddMilliseconds(1), retrievedItem.UpdatedAt);

		var retrievedItem2 = await _repository.GetItemOrDefaultAsync(item.UserId);

		Assert.NotEqual(default, retrievedItem2);
		AssertX.JsonEquals(item, retrievedItem2.Value);
		Assert.True(retrievedItem2.IsFromCache);
		Assert.Equal(retrievedItem2.UpdatedAt.ToUnixTimeMilliseconds(), (_now.AddMilliseconds(1).ToUnixTimeMilliseconds()));
		Assert.Equal(retrievedItem2.Value.Level, (nextLevel));
	}


	[Fact]
	public async Task UpdateItemTimeStampErrorTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTimeOffset.Now;
		await _repository.PutItemAsync(item.UserId, item, now);
		await Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now, now));
		//Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now, now+1)); //뭐였는지 모르겠음
	}


}

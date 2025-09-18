using Gzzz.Db.Redis;
using StackExchange.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class TestRedisRepository : RedisOptimisicRepository<TestEntity>
{
	static readonly RedisConfig _redisConfig = new RedisConfig("127.0.0.1,defaultDatabase=1,allowAdmin=true");
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

class RedisRepositoryTests
{
	TestRedisRepository _repository = new TestRedisRepository();
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

	[OneTimeSetUp]
	public Task SetupAsync()=>_repository.FlushAsync();

	[Test]
	public async Task InsertItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		await _repository.PutItemAsync(item.UserId, item, _now);
		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		Assert.That(retrievedItem, Is.Not.Default);
		AssertX.JsonEquals(item, retrievedItem.Value);
		Assert.That(retrievedItem.IsFromCache, Is.True);
		Assert.That(retrievedItem.UpdatedAt.ToUnixTimeMilliseconds(), Is.EqualTo(_now.ToUnixTimeMilliseconds()));
	}

	[Test]
	public async Task InsertDuplicatedItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTimeOffset.UtcNow;
		await _repository.PutItemAsync(item.UserId, item, now);
		var exception = Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now));
	}

	[Test]
	public async Task UpdateItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		await _repository.PutItemAsync(item.UserId, item, _now);

		var nextLevel = RandomX.GetRandom();
		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		item.Level = nextLevel;
		await _repository.PutItemAsync(item.UserId, item, _now.AddMilliseconds(1), retrievedItem.UpdatedAt);

		var retrievedItem2 = await _repository.GetItemOrDefaultAsync(item.UserId);

		Assert.That(retrievedItem2, Is.Not.Default);
		AssertX.JsonEquals(item, retrievedItem2.Value);
		Assert.That(retrievedItem2.IsFromCache, Is.True);
		Assert.That(retrievedItem2.UpdatedAt.ToUnixTimeMilliseconds(), Is.EqualTo(_now.AddMilliseconds(1).ToUnixTimeMilliseconds()));
		Assert.That(retrievedItem2.Value.Level, Is.EqualTo(nextLevel));
	}


	[Test]
	public async Task UpdateItemTimeStampErrorTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTimeOffset.Now;
		await _repository.PutItemAsync(item.UserId, item, now);
		Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now, now));
		//Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now, now+1)); //뭐였는지 모르겠음
	}


}

using Gzzz.Db.Redis;
using StackExchange.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

class RedisRepositoryTests
{
	readonly RedisConfig _redisConfig = new RedisConfig("127.0.0.1,defaultDatabase=1,allowAdmin=true");
	RedisRepository<TestEntity> _repository;

	public RedisRepositoryTests()
	{
		var partitionKey = RandomX.GetRandomText();
		var redisService = new RedisService(_redisConfig);
		_repository = new RedisRepository<TestEntity>(redisService, partitionKey);
	}

	[OneTimeSetUp]
	public async Task SetupAsync()
	{
		var connectionMultiplexer = ConnectionMultiplexer.Connect(_redisConfig.Endpoint);
		var endpoint = connectionMultiplexer.GetEndPoints()[0];
		await connectionMultiplexer.GetServer(endpoint).FlushDatabaseAsync();
	}

	[Test]
	public async Task InsertItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTime.UtcNow;
		await _repository.PutItemAsync(item.UserId, item, now);
		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		Assert.That(retrievedItem, Is.Not.Default);
		AssertX.JsonEquals(item, retrievedItem.Value);
		Assert.That(retrievedItem.IsFromCache, Is.True);
		Assert.That(retrievedItem.Timestamp, Is.EqualTo(now.ToTimescore()));
	}

	[Test]
	public async Task InsertDuplicatedItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTime.UtcNow;
		await _repository.PutItemAsync(item.UserId, item, now);
		var exception = Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now));
	}

	[Test]
	public async Task UpdateItemTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTime.UtcNow;
		await _repository.PutItemAsync(item.UserId, item, now);

		var nextLevel = RandomX.GetRandom();
		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		item.Level = nextLevel;
		await _repository.PutItemAsync(item.UserId, item, now.AddTicks(1), retrievedItem.Timestamp);

		var retrievedItem2 = await _repository.GetItemOrDefaultAsync(item.UserId);

		Assert.That(retrievedItem2, Is.Not.Default);
		AssertX.JsonEquals(item, retrievedItem2.Value);
		Assert.That(retrievedItem2.IsFromCache, Is.True);
		Assert.That(retrievedItem2.Timestamp, Is.EqualTo(now.AddTicks(1).ToTimescore()));
		Assert.That(retrievedItem2.Value.Level, Is.EqualTo(nextLevel));
	}


	[Test]
	public async Task UpdateItemTimeStampErrorTestAsync()
	{
		var item = new TestEntity { UserId = RandomX.GetRandomText(), Level = RandomX.GetRandom() };
		var now = DateTime.UtcNow;
		var timestamp = now.ToTimescore();	
		await _repository.PutItemAsync(item.UserId, item, now);
		Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now, timestamp));
		Assert.ThrowsAsync<RedisPutException>(() => _repository.PutItemAsync(item.UserId, item, now, timestamp+1));
	}


}

using Gzzz.Db;
using Gzzz.Db.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

class CompositeRepositoryTests
{
	CompositeOptimisicRepository<TestEntity, TestRedisRepository, TestDynamoDbRepository> _repository;
	readonly MockDynamoDbService _dynamoDbService = new MockDynamoDbService();

	TestDynamoDbRepository _dynamodb; 
	TestRedisRepository _redis;
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow.TrimBelowMilliseconds();
	public CompositeRepositoryTests()
	{
		_dynamodb = new TestDynamoDbRepository(_dynamoDbService);
		_redis = new TestRedisRepository();
		_repository = new (_redis, _dynamodb);
	}
	[OneTimeSetUp] public Task SetupFixtureAsync() => _dynamoDbService.CreateTableAsync();
	[OneTimeTearDown] public Task TearDownFixtureAsync() => _dynamoDbService.DeleteTableAsync();


	async Task EqualsAsync(TestEntity expected, bool redisEquals, bool dynamoDbEquals)
	{
		var redisItem = await _redis.GetItemOrDefaultAsync(expected.UserId);
		var dynamoDbItem = await _dynamodb.GetItemOrDefaultAsync(expected.UserId);

		if (redisEquals)
			AssertX.JsonEquals(expected, redisItem.Value);
		else
			AssertX.JsonNotEquals(expected, redisItem.Value);

		if (dynamoDbEquals)
			AssertX.JsonEquals(expected, dynamoDbItem.Value);
		else
			AssertX.JsonNotEquals(expected, dynamoDbItem.Value);
	}


	[Test]
	public async Task GetDefaultTestAsync()
	{
		var item = await _repository.GetItemOrDefaultAsync(RandomX.GetRandomText());
		Assert.That(item, Is.Default);
	}

	[Test]
	public async Task InsertItemTestAsync()
	{
		var item = TestEntity.CreateRandom();
		await _repository.PutItemAsync(item.UserId, item, _now);

		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		Assert.That(retrievedItem, Is.Not.Null);
		Assert.That(retrievedItem.IsFromCache, Is.True);
		Assert.That(retrievedItem.UpdatedAt, Is.EqualTo(_now));
		AssertX.JsonEquals(item, retrievedItem.Value);

		await EqualsAsync(item, true, true);
	}


	[Test]
	public async Task GetItemWithFlushTestAsync()
	{
		var item = TestEntity.CreateRandom();
		await _repository.PutItemAsync(item.UserId, item, _now);

		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId, true);
		Assert.That(retrievedItem, Is.Not.Null);
		Assert.That(retrievedItem.IsFromCache, Is.False);
		Assert.That(retrievedItem.UpdatedAt, Is.EqualTo(_now));
		AssertX.JsonEquals(item, retrievedItem.Value);

		await EqualsAsync(item, true, true);
	}


	[Test]
	public async Task UpdateErrorTestAsync()
	{
		var item = TestEntity.CreateRandom();
		var now = DateTimeOffset.Now;

		await _repository.PutItemAsync(item.UserId, item, now);

		Assert.ThrowsAsync<RedisPutException>(async () => { await _repository.PutItemAsync(item.UserId, item, now); }, "이미 등록된 아이템을 덮어쓸 수 없음");
		Assert.ThrowsAsync<RedisPutException>(async () => { await _repository.PutItemAsync(item.UserId, item, now, now); }, "동일한 timestamp 사용불가");
		Assert.ThrowsAsync<RedisPutException>(async () => { await _repository.PutItemAsync(item.UserId, item, now, now.AddMilliseconds(1)); }, "timestamp mismatch");
	}


	[Test]
	public async Task UpdatePersistentTestAsync()
	{
		var item = TestEntity.CreateRandom();
		var now = DateTimeOffset.Now;

		await _repository.PutItemAsync(item.UserId, item, now);

		item.Level = RandomX.GetRandom();
		await _repository.PersistentPutItemAsync(item.UserId, item, now.AddMilliseconds(1), now);

		await EqualsAsync(item,true,true);
	}
	[Test]
	public async Task UpdateNoPersistentTestAsync()
	{
		var item = TestEntity.CreateRandom();
		var now = DateTimeOffset.Now;

		await _repository.PutItemAsync(item.UserId, item, now);

		item.Level = RandomX.GetRandom();
		await _repository.PutItemAsync(item.UserId, item, now.AddMilliseconds(1), now);

		await EqualsAsync(item,true,false);
	}
}

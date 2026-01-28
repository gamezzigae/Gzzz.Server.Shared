using Gzzz.Db;
using Gzzz.Db.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class CompositeRepositoryTests : IAsyncLifetime
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

	public async ValueTask InitializeAsync() => await _dynamoDbService.CreateTableAsync();
	public async ValueTask DisposeAsync() => await _dynamoDbService.DeleteTableAsync();


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


	[Fact]
	public async Task GetDefaultTestAsync()
	{
		var item = await _repository.GetItemOrDefaultAsync(RandomX.GetRandomText());
		Assert.Equal(default, item);
	}

	[Fact]
	public async Task InsertItemTestAsync()
	{
		var item = TestEntity.CreateRandom();
		await _repository.PutItemAsync(item.UserId, item, _now);

		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId);
		Assert.NotNull(retrievedItem);
		Assert.True(retrievedItem.IsFromCache);
		Assert.Equal(retrievedItem.UpdatedAt, _now);
		AssertX.JsonEquals(item, retrievedItem.Value);

		await EqualsAsync(item, true, true);
	}


	[Fact]
	public async Task GetItemWithFlushTestAsync()
	{
		var item = TestEntity.CreateRandom();
		await _repository.PutItemAsync(item.UserId, item, _now);

		var retrievedItem = await _repository.GetItemOrDefaultAsync(item.UserId, true);
		Assert.NotNull(retrievedItem);
		Assert.False(retrievedItem.IsFromCache);
		Assert.Equal(retrievedItem.UpdatedAt, _now);
		AssertX.JsonEquals(item, retrievedItem.Value);

		await EqualsAsync(item, true, true);
	}


	[Fact]
	public async Task UpdateErrorTestAsync()
	{
		var item = TestEntity.CreateRandom();
		var now = DateTimeOffset.Now;

		await _repository.PutItemAsync(item.UserId, item, now);

		await Assert.ThrowsAsync<RedisPutException>(()=> _repository.PutItemAsync(item.UserId, item, now));
		await Assert.ThrowsAsync<RedisPutException>(()=>  _repository.PutItemAsync(item.UserId, item, now, now) );
		await Assert.ThrowsAsync<RedisPutException>(()=>  _repository.PutItemAsync(item.UserId, item, now, now.AddMilliseconds(1)));
	}


	[Fact]
	public async Task UpdatePersistentTestAsync()
	{
		var item = TestEntity.CreateRandom();
		var now = DateTimeOffset.Now;

		await _repository.PutItemAsync(item.UserId, item, now);

		item.Level = RandomX.GetRandom();
		await _repository.PersistentPutItemAsync(item.UserId, item, now.AddMilliseconds(1), now);

		await EqualsAsync(item,true,true);
	}
	[Fact]
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

using Amazon.DynamoDBv2.Model;
using Gzzz.Db.DynamoDb;
using Gzzz.Server.Shared.Tests.Db;
using Newtonsoft.Json;

namespace Gzzz.Server.Shared.Tests.DbTests;



class DynamoDbRepositoryTests
{
	[OneTimeSetUp] public Task SetupFixtureAsync() => _dynamoDbService.CreateTableAsync();
	[OneTimeTearDown] public Task TearDownFixtureAsync() => _dynamoDbService.DeleteTableAsync();

	readonly TestDynamoDbRepository _testRepository;
	readonly MockDynamoDbService _dynamoDbService = new MockDynamoDbService();
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow.TrimBelowMilliseconds();
	public DynamoDbRepositoryTests()
	{
		_testRepository = new TestDynamoDbRepository(_dynamoDbService);
	}
	

	[Test]
	public async Task InsertItemTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		await _testRepository.PutItemAsync(item.UserId, item, _now);

		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);

		AssertX.JsonEquals(item, retrievedItem.Value);
		Assert.That(retrievedItem.UpdatedAt, Is.EqualTo(_now));
		Assert.That(retrievedItem.IsFromCache, Is.False);
	}


	[Test]
	public async Task InsertExistsKeyErrorTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		await _testRepository.PutItemAsync(item.UserId, item, _now);
		Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _testRepository.PutItemAsync(item.UserId, item, _now));
	}

	[Test]
	public async Task UpdateItemTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		await _testRepository.PutItemAsync(item.UserId, item, _now);
		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);

		item.Level++;

		var now2 = _now.AddMilliseconds(1);
		await _testRepository.PutItemAsync(item.UserId, item, now2, retrievedItem.UpdatedAt);

		var retrievedItem2 = await _testRepository.GetItemOrDefaultAsync(item.UserId);
		AssertX.JsonEquals(item, retrievedItem2.Value);
	}


	[Test]
	public async Task UpdateEqualTimestampErrorTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		await _testRepository.PutItemAsync(item.UserId, item, _now);
		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);
		item.Level++;
		Assert.ThrowsAsync<ArgumentException>(() => _testRepository.PutItemAsync(item.UserId, item, _now, retrievedItem.UpdatedAt));
	}
	[Test]
	public async Task UpdateCheckTimestampErrorTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		await _testRepository.PutItemAsync(item.UserId, item, _now);
		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);
		item.Level++;
		Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _testRepository.PutItemAsync(item.UserId, item, _now, retrievedItem.UpdatedAt.AddMilliseconds(-1)));
	}
}

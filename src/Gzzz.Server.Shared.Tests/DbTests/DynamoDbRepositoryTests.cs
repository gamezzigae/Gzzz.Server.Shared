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

		var now = DateTime.Now;
		await _testRepository.PutItemAsync(item, now);

		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);

		AssertX.JsonEquals(item, retrievedItem.Value);
		Assert.That(retrievedItem.Timestamp, Is.EqualTo(now.ToTimescore()));
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

		var now = DateTime.Now;
		await _testRepository.PutItemAsync(item, now);
		Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _testRepository.PutItemAsync(item, now));
	}

	[Test]
	public async Task UpdateItemTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		var now = DateTime.Now;
		await _testRepository.PutItemAsync(item, now);
		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);

		item.Level++;

		now = now.AddTicks(1);
		await _testRepository.PutItemAsync(item, now, retrievedItem.Timestamp);

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

		var now = DateTime.Now;
		await _testRepository.PutItemAsync(item, now);
		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);
		item.Level++;
		Assert.ThrowsAsync<ArgumentException>(() => _testRepository.PutItemAsync(item, now, retrievedItem.Timestamp));
	}
	[Test]
	public async Task UpdateCheckTimestampErrorTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		var now = DateTime.Now;
		await _testRepository.PutItemAsync(item, now);
		var retrievedItem = await _testRepository.GetItemOrDefaultAsync(item.UserId);
		item.Level++;
		Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _testRepository.PutItemAsync(item, now, retrievedItem.Timestamp-1));
	}
}

using Amazon.DynamoDBv2.Model;
using Gzzz.Db.DynamoDb;
using Newtonsoft.Json;

namespace Gzzz.Server.Shared.Tests.DbTests;



public class DynamoDbRepositoryTests : IAsyncLifetime
{
	public async ValueTask InitializeAsync() =>await _dynamoDbService.CreateTableAsync();
	public async ValueTask DisposeAsync() => await _dynamoDbService.DeleteTableAsync();

	readonly TestDynamoDbRepository _testRepository;
	readonly MockDynamoDbService _dynamoDbService = new MockDynamoDbService();
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow.TrimBelowMilliseconds();
	public DynamoDbRepositoryTests()
	{
		_testRepository = new TestDynamoDbRepository(_dynamoDbService);
	}
	

	[Fact]
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
		Assert.Equal(_now, retrievedItem.UpdatedAt);
		Assert.False(retrievedItem.IsFromCache);
	}


	[Fact]
	public async Task InsertExistsKeyErrorTestAsync()
	{
		var item = new TestEntity
		{
			UserId = RandomX.GetRandomText(),
			Level = RandomX.GetRandom()
		};

		await _testRepository.PutItemAsync(item.UserId, item, _now);
		await Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _testRepository.PutItemAsync(item.UserId, item, _now));
	}

	[Fact]
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


	[Fact]
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
		await Assert.ThrowsAsync<ArgumentException>(() => _testRepository.PutItemAsync(item.UserId, item, _now, retrievedItem.UpdatedAt));
	}
	[Fact]
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
		await Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _testRepository.PutItemAsync(item.UserId, item, _now, retrievedItem.UpdatedAt.AddMilliseconds(-1)));
	}
}

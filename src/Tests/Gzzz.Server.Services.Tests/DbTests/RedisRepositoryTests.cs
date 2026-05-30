using Gzzz.Db.Redis;

namespace Gzzz.Server.Shared.Tests.DbTests;

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
		Assert.Equal(retrievedItem.UpdatedAt.ToLong(), _now.ToLong());
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
		Assert.Equal(retrievedItem2.UpdatedAt.ToLong(), _now.AddMilliseconds(1).ToLong());
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

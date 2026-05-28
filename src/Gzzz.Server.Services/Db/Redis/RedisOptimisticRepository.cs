using Gzzz.Serialize;
using StackExchange.Redis;

namespace Gzzz.Db.Redis;

public class RedisOptimisticRepository<T>
	where T : class
{
	readonly string _sortedSetKey;
	readonly string _partitionKey;
	readonly RedisService _redisService;
	readonly ITextSerializer<T> _textSerializer;

	public RedisOptimisticRepository(RedisService redisService, string partitionKey, bool useCompression=true)
	{
		_partitionKey = partitionKey+":";
		_sortedSetKey = _partitionKey + "_Timestamps";
		_redisService = redisService;
		_textSerializer = useCompression? new GZipJsonSerializer<T>() : new JsonSerializer<T>();
	}


	public async Task RemoveItemsAsync(string[] keys)
	{
		var db = _redisService.GetDatabase();

		var redieKeys = keys.Select(item => (RedisKey)(_partitionKey + item)).ToArray();
		await db.SortedSetRemoveAsync(_sortedSetKey, keys.Select(key => (RedisValue)key).ToArray());
		await db.KeyDeleteAsync(redieKeys);
	}

	public async Task<Dictionary<string, OptimisticRecord<T>>> GetExpiredItemsAsync(DateTimeOffset expireAt, int take = 10)
	{
		var maxTimestamp = expireAt.UtcTicks;
		var db = _redisService.GetDatabase();
		var sortedSetEntries = await db.SortedSetRangeByScoreWithScoresAsync(
			key: _sortedSetKey,
			start: double.NegativeInfinity,
			stop: maxTimestamp,
			take: take
		);

		var expiredKeys = sortedSetEntries.Select(item => item.Element.ToString()).ToArray();
		var expiredRedisKeys = expiredKeys.Select(item => new RedisKey(_partitionKey + item)).ToArray();
		var items = await db.StringGetAsync(expiredRedisKeys);

		var result = new Dictionary<string, OptimisticRecord<T>>();
		for (int i = 0; i < items.Length; i++)
		{
			var itemKey = expiredKeys[i];
			var redisValue = items[i];
			var deserializedItem = _textSerializer.Deserialize(redisValue);
			var timestamp = (long)sortedSetEntries[i].Score;
			var record = new OptimisticRecord<T>(itemKey, deserializedItem, DateTimeOffset.FromUnixTimeMilliseconds(timestamp), true);
			result.Add(itemKey, record);
		}

		return result;
	}

	
	public async Task PutItemAsync(string key, T item, DateTimeOffset now, DateTimeOffset updatedAt = default)
	{
		var nowUnixMs = now.UtcTicks;
		var updatedAtUnixMs = updatedAt.UtcTicks;
		if (nowUnixMs <= updatedAtUnixMs)
		{
			throw new RedisPutException("time condition error");
		}
		var redisKey = _partitionKey + key;

		var serializedItem = _textSerializer.Serialize(item);

        var transaction = _redisService.CreateTransaction();

		
		if (updatedAt == default)
		{
			transaction.AddCondition(Condition.SortedSetNotContains(_sortedSetKey, key));
			transaction.AddCondition(Condition.KeyNotExists(redisKey));
		}
        else
		{
			transaction.AddCondition(Condition.SortedSetEqual(_sortedSetKey, key, updatedAtUnixMs));
			transaction.AddCondition(Condition.KeyExists(redisKey));
		}
        var sortedSetAddTask = transaction.SortedSetAddAsync(_sortedSetKey, key, nowUnixMs);
        var setAddTask = transaction.StringSetAsync(redisKey, serializedItem);
        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new RedisPutException("not committed");
        }
	}

	public Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string itemKey1, string itemKey2) => GetItemOrDefaultAsync($"{itemKey1}:{itemKey2}");

	public async Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string itemKey)
	{
		var db = _redisService.GetDatabase();

		var redisKey = _partitionKey + itemKey;
		var timestamp = await db.SortedSetScoreAsync(_sortedSetKey, itemKey);
		if (timestamp.HasValue==false)
			return default;
		
		var item = await db.StringGetAsync(redisKey);
		var deserializedItem = _textSerializer.Deserialize(item);
		return new(itemKey, deserializedItem, DateTimeOffset.FromUnixTimeMilliseconds( (long)timestamp.Value), true);
	}

	public async Task DeleteItemAsync(string itemKey)
	{
		var db = _redisService.GetDatabase();
		await db.SortedSetRemoveAsync(_sortedSetKey, itemKey);
		await db.KeyDeleteAsync(_partitionKey + itemKey);
	}
}

public class RedisPutException : Exception
{

	public RedisPutException(string message) : base(message)
	{
	}
}

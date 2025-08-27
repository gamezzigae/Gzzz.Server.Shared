using Gzzz.Db.DynamoDb;
using Gzzz.Db.Redis;

namespace Gzzz.Db;

public class CompositeOptimisicRepository<T, TRedisRepository, TDynamoDbRepository> : IOptimisticRepository<T>
	where TRedisRepository : RedisOptimisicRepository<T>
	where TDynamoDbRepository : DynamoDbOptimisicRepository<T>
	where T : class

{
	readonly TRedisRepository _redisRepository;
	readonly TDynamoDbRepository _dynamoDbRepository;

	public CompositeOptimisicRepository(TRedisRepository redisRepository, TDynamoDbRepository dynamoDbRepository)
	{
		_redisRepository = redisRepository;
		_dynamoDbRepository = dynamoDbRepository;
	}


	public Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string key)=> GetItemOrDefaultAsync(key, false);
	public async Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string key, bool flushCache)
	{
		if (flushCache)
		{
			await _redisRepository.DeleteItemAsync(key);
		}
		else
		{ 
			var redisRecord = await _redisRepository.GetItemOrDefaultAsync(key);
			if (redisRecord.IsNotDefault())
			{
				return redisRecord;
			}
		}

		var dynamoDbRecord = await _dynamoDbRepository.GetItemOrDefaultAsync(key);
		if (dynamoDbRecord.IsDefault())
			return default;

		await _redisRepository.PutItemAsync(key, dynamoDbRecord.Value, dynamoDbRecord.Timestamp, 0);

		return dynamoDbRecord;
	}


	public Task<long> PutItemAsync(string key, T value, DateTime now, long checkTimestamp = 0)=> checkTimestamp == 0
		? PersistentPutItemAsync( key,  value,  now)
		: _redisRepository.PutItemAsync(key, value, now, checkTimestamp);

	public async Task<long> PersistentPutItemAsync(string key, T value, DateTime now, long checkTimestamp = 0)
	{
		var result = await _redisRepository.PutItemAsync(key, value, now, checkTimestamp);
		await _dynamoDbRepository.PutItemAsync(value, now, checkTimestamp);
		return result;
	}
}

using Gzzz.Db.DynamoDb;
using Gzzz.Db.Redis;

namespace Gzzz.Db;
public class CompositeOptimisicRepository<T, TRedisRepository, TDynamoDbRepository>
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


	public async Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string key, bool flushCache = false)
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


	public async Task PutItemAsync(string key, T value, DateTime now, long checkTimestamp=0, bool isPersistent=false)
	{
		await _redisRepository.PutItemAsync(key, value, now, checkTimestamp);

		if (isPersistent || checkTimestamp == 0)
		{
			await _dynamoDbRepository.PutItemAsync(value, now, checkTimestamp);
		}
	}
}

using Amazon.DynamoDBv2.Model;
using Gzzz.Serialize;
using StackExchange.Redis;

namespace Gzzz.Db;

public record OptimisticRecord<T>(
    string Key,
    T Value,
	DateTimeOffset UpdatedAt, // Unix timestamp in milliseconds
    bool IsFromCache
);

public class DynamoDbRecord<T>
{
	public bool IsFromCache { get; }
	public Dictionary<string, AttributeValue> Attributes { get; }

	public DynamoDbRecord(bool isFromCache, Dictionary<string,AttributeValue> attributes)
	{
		this.IsFromCache = isFromCache;
		Attributes = attributes;
	}


	public RedisValue ToRedisValue()
	{
		throw new NotImplementedException();
	}
}

using Amazon.DynamoDBv2.Model;
using Gzzz.Db.DynamoDb;
using Gzzz.Serialize;
using StackExchange.Redis;

namespace Gzzz.Db;

public record OptimisticRecord<T>(
    string Key,
    T Value,
	DateTimeOffset UpdatedAt, // Unix timestamp in milliseconds
    bool IsFromCache
);

public class DynamoDbRecord
{
	public bool IsFromCache { get; }
	public Dictionary<string, AttributeValue> Attributes { get; }

	public DynamoDbRecord(bool isFromCache, Dictionary<string,AttributeValue> attributes)
	{
		this.IsFromCache = isFromCache;
		Attributes = attributes;
	}

	public byte[] ToRedisValue()
	{
		var bytes = Json.SerializeBytes(Attributes);
		var compressed = Zstd.Compress(bytes);
		return compressed;
	}
}

/*
| Method          | Mean     | Error   | StdDev   | Gen0   | Gen1   | Allocated |
|---------------- |---------:|--------:|---------:|-------:|-------:|----------:|
| ToRedisValue1   | 321.2 us | 6.39 us | 15.18 us | 7.3242 | 0.4883 |  61.07 KB | 그냥 전부 저장
| ToRedisValue2   | 317.4 us | 6.24 us |  7.18 us | 9.7656 | 0.4883 |  81.89 KB | pk,sk,ua빼고 저장, 용량은 100byte미만차이

 */

using Amazon.DynamoDBv2.Model;

namespace Gzzz.Db.DynamoDb;

public class DynamoDbOptimisticRepository<T> : IOptimisticRepository<T>
	where T : class
{
	readonly DynamoDbService _dynamoDbService;
	readonly string _partitionKey;
	readonly AttributeValue _partitionKeyAttributeValue;

	public DynamoDbOptimisticRepository(DynamoDbService dynamoDbService, string partitionKey)
	{
		_dynamoDbService = dynamoDbService;
		_partitionKey = partitionKey;
		_partitionKeyAttributeValue = new AttributeValue(partitionKey);
	}

	public async Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string key)
	{
		var attributeMap = await _dynamoDbService.GetAttirubtesAsync(_partitionKey, key);
		if (attributeMap==default)
			return default;

		var value = AttributeMap.ConvertTo<T>(attributeMap);
		var record = new OptimisticRecord<T>(key, value, attributeMap.GetUpdatedAt(), false);

		return record;
	}
	public async Task PutItemAsync(string key, T item, DateTimeOffset now, DateTimeOffset updatedAt = default)
	{
		var attributeMap = AttributeMap.ConvertFrom(item);
		attributeMap.Add(DynamoDbKeys.PartitionKey, _partitionKeyAttributeValue);
		attributeMap.Add(DynamoDbKeys.SortKey, new AttributeValue(key));
		await _dynamoDbService.PutItemAsync(attributeMap, now, updatedAt);
	}
}

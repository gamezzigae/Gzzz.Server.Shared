using Amazon.DynamoDBv2.Model;

namespace Gzzz.DynamoDb;

public class DynamoDbRepository<T>
{
	readonly DynamoDbService _dynamoDbService;
	readonly string _partitionKey;
	readonly AttributeValue _partitionKeyAttributeValue;
	readonly string _sortKeyFieldName;

	public DynamoDbRepository(DynamoDbService dynamoDbService, string partitionKey, string sortKeyFieldName)
	{
		_dynamoDbService = dynamoDbService;
		_partitionKey = partitionKey;
		_partitionKeyAttributeValue = new AttributeValue(partitionKey);
		_sortKeyFieldName = sortKeyFieldName;
	}
	public async ValueTask<OptimisticRecord<T>> GetItemOrDefaultAsync(string sortKey)
	{
		var attributeMap = await _dynamoDbService.GetAttirubtesAsync(_partitionKey, sortKey);
		if (attributeMap==default)
			return default;

		var timestamp = long.Parse(attributeMap["TS"].N);
		var value = AttributeMap.ConvertTo<T>(attributeMap);
		var record = new OptimisticRecord<T>(sortKey, value, timestamp, false);

		return record;
	}
	public async ValueTask<long> PutItemAsync(T item, DateTime now, long checkTimestamp, DynamoDbCondition dynamoDbCondition = DynamoDbCondition.Update)
	{
		var attributeMap = AttributeMap.ConvertFrom(item);
		attributeMap.Add("PK", _partitionKeyAttributeValue);
		attributeMap.Add("SK", attributeMap[_sortKeyFieldName]);
		var nextTimestamp = now.ToTimescore();
		attributeMap.Add("TS", new AttributeValue { N = nextTimestamp.ToString() });
		await _dynamoDbService.PutItemAsync(attributeMap, new AttributeValue() { N= checkTimestamp .ToString() }, dynamoDbCondition);
		return nextTimestamp;
	}
}

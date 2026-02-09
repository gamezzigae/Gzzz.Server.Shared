using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.DynamoDBv2.Model;

namespace Gzzz.Db.DynamoDb;

public class DynamoDbConfig
{
	public static readonly string EnvironmentVariableName = "ZZ_DYNAMODB_CONFIG";

	public string TableName { get; init; }
	public string ServiceURL { get; init; }
}

public class DynamoDbService
{
	public AmazonDynamoDBClient GetClient() => _client;
	protected readonly AmazonDynamoDBClient _client;
	protected readonly DynamoDbConfig _dynamoDbConfig;
	public readonly string TableName;

	public DynamoDbService(AWSCredentials awsCredentials, DynamoDbConfig dynamoDbConfig)
	{
		this._dynamoDbConfig = dynamoDbConfig;
		this.TableName = dynamoDbConfig.TableName;
		this._client = dynamoDbConfig.ServiceURL == default
		? new(awsCredentials)
		: new(awsCredentials, new AmazonDynamoDBConfig() { ServiceURL = dynamoDbConfig.ServiceURL });
	}


	
	public async Task<Dictionary<string, AttributeValue>> GetAttirubtesAsync(string partitionKey, string sortKey)
	{
		var keys = AttributeMap.CreateKeys(partitionKey, sortKey);
		var request = new GetItemRequest()
		{
			TableName = this.TableName,
			Key = keys,
		};
		var response = await _client.GetItemAsync(request);
		if (response.IsItemSet == false)
			return default;

		return response.Item;
	}


	public async Task PutItemAsync(Dictionary<string, AttributeValue> attributeMap, DateTimeOffset now, DateTimeOffset updatedAt = default)
    {
        if (attributeMap.ContainsKey(DynamoDbKeys.PartitionKey) == false)
            throw new ArgumentException("attributeMap must contain a 'PK'");
        if (attributeMap.ContainsKey(DynamoDbKeys.SortKey) == false)
            throw new ArgumentException("attributeMap must contain a 'SK'");

		var nowUnixMs = now.ToUnixTimeMilliseconds();
		var updatedAtUnixms = updatedAt.ToUnixTimeMilliseconds();
		if (nowUnixMs <= updatedAtUnixms)
		{
			throw new ArgumentException("dynamodb putitem time condition error");
		}
		attributeMap.Add(DynamoDbKeys.UpdatedAt, new AttributeValue() { N = nowUnixMs.ToString() });

		var condition = updatedAt == default ? DynamoDbCondition.Insert : DynamoDbCondition.Update;
		
        var request = new PutItemRequest()
		{
			TableName = this.TableName,
			Item = attributeMap,
			ConditionExpression = GetCondition(condition),
		};

		if (condition == DynamoDbCondition.Update)
		{
            request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
			{
				{ ":uat", new () { N = updatedAtUnixms.ToString() } }
			};
		}

		await _client.PutItemAsync(request);
		attributeMap.Remove(DynamoDbKeys.UpdatedAt); //다시지워준다.
	}

	static string GetCondition(DynamoDbCondition condition) => condition switch
	{
		DynamoDbCondition.Insert => "attribute_not_exists(PK) and attribute_not_exists(SK)",
		DynamoDbCondition.Update => "attribute_exists(PK) and attribute_exists(SK) and UA = :uat",
		//DynamoDbCondition.InsertOrUpdate => "TS = :uat or (attribute_not_exists(PK) and attribute_not_exists(SK))",
		_ => throw new ArgumentOutOfRangeException("invalid DynamoDbCondition:" + condition.ToString())
	};
}
public enum DynamoDbCondition
{
	Update,
	Insert,
	InsertOrUpdate,
}

public static class DynamoDbKeys
{
	public static readonly string PartitionKey = "PK";
	public static readonly string SortKey = "SK";
	public static readonly string UpdatedAt = "UA";

}

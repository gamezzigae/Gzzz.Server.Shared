using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.DynamoDBv2.Model;

namespace Gzzz.Db.DynamoDb;

public record DynamoDbConfig(string TableName, string ServiceURL = null);

public class DynamoDbService
{
	public AmazonDynamoDBClient GetClient() => _client;
	protected readonly AmazonDynamoDBClient _client;
	public readonly string TableName;

	public DynamoDbService(AWSCredentials awsCredentials, DynamoDbConfig dynamoDbConfiguration)
	{
		this.TableName = dynamoDbConfiguration.TableName;
		this._client = dynamoDbConfiguration.ServiceURL == default
		? new(awsCredentials)
		: new(awsCredentials, new AmazonDynamoDBConfig() { ServiceURL = dynamoDbConfiguration.ServiceURL });
	}


	
	public async Task<Dictionary<string, AttributeValue>> GetAttirubtesAsync(string partitionKey, string sortKey, string projectionExpression = null)
	{
		var keys = AttributeMap.CreateKeys(partitionKey, sortKey);
		var request = new GetItemRequest()
		{
			TableName = this.TableName,
			Key = keys,
			ProjectionExpression = projectionExpression,
		};
		var response = await _client.GetItemAsync(request);
		if (response.IsItemSet == false)
			return default;

		return response.Item;
	}


	public async Task PutItemAsync(Dictionary<string, AttributeValue> attributeMap, AttributeValue checkTimestamp = null)
    {
        if (attributeMap["PK"] == null)
            throw new ArgumentException("attributeMap must contain a 'PK'");
        if (attributeMap["SK"] == null)
            throw new ArgumentException("attributeMap must contain a 'SK'");
        if (attributeMap["TS"] == null)
            throw new ArgumentException("attributeMap must contain a 'TS'");

        var condition = checkTimestamp == null ? DynamoDbCondition.Insert : DynamoDbCondition.Update;
		
        var request = new PutItemRequest()
		{
			TableName = this.TableName,
			Item = attributeMap,
			ConditionExpression = GetCondition(condition),
		};

		if (condition == DynamoDbCondition.Update)
		{
			if(attributeMap["TS"].N == checkTimestamp.N)
            {
                throw new ArgumentException("before/after timestamp°¡ °°À½");
            }
            request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>() { { ":uat", checkTimestamp } };
		}

		await _client.PutItemAsync(request);
	}

	static string GetCondition(DynamoDbCondition condition) => condition switch
	{
		DynamoDbCondition.Insert => "attribute_not_exists(PK) and attribute_not_exists(SK)",
		DynamoDbCondition.Update => "attribute_exists(PK) and attribute_exists(SK) and TS = :uat",
		//DynamoDbCondition.InsertOrUpdate => "TS = :uat or (attribute_not_exists(PK) and attribute_not_exists(SK))",
		_ => throw new Exception("invalid DynamoDbCondition:" + condition.ToString())
	};
}
public enum DynamoDbCondition
{
	Update,
	Insert,
	InsertOrUpdate,
}

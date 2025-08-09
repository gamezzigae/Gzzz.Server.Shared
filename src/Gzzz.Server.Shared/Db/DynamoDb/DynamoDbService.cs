using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.DynamoDBv2.Model;

namespace Gzzz.DynamoDb;

public record DynamoDbConfiguration(string TableName, string ServiceURL = null);

public class DynamoDbService
{
	public AmazonDynamoDBClient GetClient() => _client;
	protected readonly AmazonDynamoDBClient _client;
	public readonly string TableName;

	public DynamoDbService(DynamoDbConfiguration dynamoDbConfiguration)
	{
		this.TableName = dynamoDbConfiguration.TableName;
		var credentials = FallbackCredentialsFactory.GetCredentials();
		this._client = dynamoDbConfiguration.ServiceURL == default
		? new(credentials)
		: new(credentials, new AmazonDynamoDBConfig() { ServiceURL = dynamoDbConfiguration.ServiceURL });
		
	}


	public async ValueTask<Dictionary<string, AttributeValue>> GetAttirubtesAsync(string partitionKey, string sortKey, string projectionExpression = null)
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


	public async ValueTask PutItemAsync(Dictionary<string, AttributeValue> attributeMap, AttributeValue checkTimestamp, DynamoDbCondition dynamoDbCondition = DynamoDbCondition.Update)
	{
		var request = new PutItemRequest()
		{
			TableName = this.TableName,
			Item = attributeMap,
			ConditionExpression = GetCondition(dynamoDbCondition),
		};

		if (dynamoDbCondition != DynamoDbCondition.Insert)
		{
			request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>() { { ":uat", checkTimestamp } };
		}

		await _client.PutItemAsync(request);
	}

	static string GetCondition(DynamoDbCondition condition) => condition switch
	{
		DynamoDbCondition.Update => "attribute_exists(PK) and attribute_exists(SK) and TS = :uat",
		DynamoDbCondition.Insert => "attribute_not_exists(PK) and attribute_not_exists(SK)",
		DynamoDbCondition.InsertOrUpdate => "TS = :uat or (attribute_not_exists(PK) and attribute_not_exists(SK))",
		_ => throw new Exception("invalid DynamoDbCondition:" + condition.ToString())
	};
}
public enum DynamoDbCondition
{
	Update,
	Insert,
	InsertOrUpdate,
}

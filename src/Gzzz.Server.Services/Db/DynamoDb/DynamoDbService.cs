using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Gzzz.Services.Authentication;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Gzzz.Db.DynamoDb;

public class DynamoDbService
{
	public readonly AmazonDynamoDBClient Client;
	public readonly string TableName;
	readonly ITextLogger _logger;
	readonly ReturnConsumedCapacity _returnConsumedCapacity;

	public DynamoDbService(AWSCredentials awsCredentials, DynamoDbConfig dynamoDbConfig, ITextLogger logger)
	{
		_logger = logger;
		_returnConsumedCapacity = dynamoDbConfig.ReturnConsumedCapacity;
		this.TableName = dynamoDbConfig.TableName;

		if(dynamoDbConfig.ServiceURL == default)
		{
			this.Client = new(awsCredentials);
		}
		else
		{
			this.Client = new(awsCredentials, new AmazonDynamoDBConfig() { ServiceURL = dynamoDbConfig.ServiceURL });
		}

	}

	public async Task<Dictionary<string, AttributeValue>> GetAsync(string partitionKey, string sortKey, string projectionExpression = null)
	{
		var keys = new Dictionary<string, AttributeValue>()
		{
			{ DynamoDbKeys.PartitionKey, new(partitionKey) },
			{ DynamoDbKeys.SortKey, new(sortKey) }
		};

		var request = new GetItemRequest()
		{
			TableName = this.TableName,
			Key = keys,
			ReturnConsumedCapacity = _returnConsumedCapacity,
			ProjectionExpression = projectionExpression
		};

		try
		{
			var response = await Client.GetItemAsync(request);

			if (_returnConsumedCapacity != null)
			{
				_logger.Write($"{{\"rcu\":{response.ConsumedCapacity.CapacityUnits}}}");
			}

			if (response.IsItemSet == false)
				return default;

			return response.Item;
		}
		catch (Exception)
		{
			throw new HttpException(500, "dynamodb getitem error");
		}
	}


	async Task PutItemAsync(PutItemRequest putItemRequest)
	{
		try
		{
			var response = await Client.PutItemAsync(putItemRequest);
			if (_returnConsumedCapacity != null)
			{
				_logger.Write($"{{\"wcu\":{response.ConsumedCapacity.CapacityUnits}}}");
			}
		}
		catch (ConditionalCheckFailedException ccfex) when (ccfex.Message == "The conditional request failed")
		{
			throw new HttpException(500, "condition error");
		}
	}

	public async Task InsertAsync(Dictionary<string, AttributeValue> attributeMap)
	{
		var request = new PutItemRequest()
		{
			TableName = this.TableName,
			Item = attributeMap,
			ConditionExpression = "attribute_not_exists(PK) and attribute_not_exists(SK)",
			ReturnConsumedCapacity = _returnConsumedCapacity,
		};
		await PutItemAsync(request);
	}



	public async Task PutAsync(Dictionary<string, AttributeValue> attributeMap, DateTimeOffset now)
	{
        if (attributeMap.ContainsKey(DynamoDbKeys.PartitionKey) == false)
            throw new ArgumentException("attributeMap must contain a 'PK'");
		if (attributeMap.ContainsKey(DynamoDbKeys.SortKey) == false)
			throw new ArgumentException("attributeMap must contain a 'SK'");
		if (attributeMap.TryGetValue(DynamoDbKeys.UpdatedAt, out var lastUpdatedAt) == false)
			throw new ArgumentException("attributeMap must contain a 'UA'");

		var newUpdatedAt = now.Ticks;
		if (newUpdatedAt <= long.Parse(lastUpdatedAt.N))
		{
			throw new ArgumentException("dynamodb put item time condition error");
		}

		attributeMap[DynamoDbKeys.UpdatedAt] = new AttributeValue() { N = newUpdatedAt.ToString() };
		
        var request = new PutItemRequest()
		{
			TableName = this.TableName,
			Item = attributeMap,
			ConditionExpression = "UA=:ua",
			ExpressionAttributeValues = new() { { ":ua", lastUpdatedAt } },
			ReturnConsumedCapacity = _returnConsumedCapacity,
		};

		await PutItemAsync(request);
	}

	public async Task UpdateItemAsync(UpdateItemRequest request)
	{
		request.ReturnConsumedCapacity = _returnConsumedCapacity;
		var response = await Client.UpdateItemAsync(request);

		if (_returnConsumedCapacity != null)
		{
			_logger.Write($"{{\"wcu\":{response.ConsumedCapacity.CapacityUnits}}}");
		}
	}

	public async Task UpdateItemAsync(Dictionary<string, AttributeValue> attributeMap, DateTimeOffset now)
	{

		if (attributeMap.TryGetValue(DynamoDbKeys.PartitionKey, out var pk) == false)
			throw new ArgumentException("attributeMap must contain a 'PK'");
		if (attributeMap.TryGetValue(DynamoDbKeys.SortKey, out var sk) == false)
			throw new ArgumentException("attributeMap must contain a 'SK'");
		if (attributeMap.TryGetValue(DynamoDbKeys.UpdatedAt, out var lastUpdatedAt) == false)
			throw new ArgumentException("attributeMap must contain a 'UA'");

		var newUpdatedAt = now.Ticks;
		if (newUpdatedAt <= long.Parse(lastUpdatedAt.N))
		{
			throw new ArgumentException("dynamodb update item time condition error");
		}

		var expressionAttributeNames = new Dictionary<string, string>(attributeMap.Count);
		var expressionAttributeValues = new Dictionary<string, AttributeValue>(attributeMap.Count + 1);
		expressionAttributeValues.Add(":ua", lastUpdatedAt);

		var sb = new StringBuilder("SET ");
		int i = 0;
		foreach (var (key, value) in attributeMap)
		{
			if (key == DynamoDbKeys.PartitionKey || key == DynamoDbKeys.SortKey)
				continue;

			i++;
			var namePlaceholder = string.Concat("#k", i);
			var valuePlaceholder = string.Concat(":v", i);

			sb.Append(namePlaceholder);
			sb.Append('=');
			sb.Append(valuePlaceholder);
			sb.Append(',');
			expressionAttributeNames.Add(namePlaceholder, key);
			expressionAttributeValues.Add(valuePlaceholder, value);
		}
		sb.Length--;

		var request = new UpdateItemRequest
		{
			TableName = this.TableName,
			Key = new Dictionary<string, AttributeValue>
			{
				["PK"] = pk,
				["SK"] = sk
			},
			UpdateExpression = sb.ToString(),
			ExpressionAttributeNames = expressionAttributeNames,
			ExpressionAttributeValues = expressionAttributeValues,
			ConditionExpression = "UA=:ua",
		};

		await UpdateItemAsync(request);
	}
}

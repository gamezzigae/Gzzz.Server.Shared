using Amazon.DynamoDBv2.Model;
using Gzzz.Db.DynamoDb;

namespace Gzzz.Services.Authentication;

public interface IUserAuthenticatedInfoUpdater
{
	Task UpdateAuthenticatedInfoAsync(string userId, DateTimeOffset createdAt);
}
public class DefaultTokenUpdateService : IUserAuthenticatedInfoUpdater
{
	Task IUserAuthenticatedInfoUpdater.UpdateAuthenticatedInfoAsync(string userId, DateTimeOffset createdAt) => Task.CompletedTask;
}

public class DynamoDbAuthenticatedInfoUpdater : IUserAuthenticatedInfoUpdater
{
	static readonly AttributeValue _emptyValue = new AttributeValue("");
	readonly DynamoDbService _dynamoDbService;
	public DynamoDbAuthenticatedInfoUpdater(DynamoDbService dynamoDbService)
	{
		_dynamoDbService = dynamoDbService;
	}

	public async Task UpdateAuthenticatedInfoAsync(string userId, DateTimeOffset createdAt)
	{

		var request = new UpdateItemRequest
		{
			TableName = _dynamoDbService.TableName,
			Key = new Dictionary<string, AttributeValue>
			{
				[DynamoDbKeys.PartitionKey] = new AttributeValue(DynamoDbTable.User),
				[DynamoDbKeys.SortKey] = new AttributeValue(userId),
				//[DynamoDbKeys.LastRequestId] = new AttributeValue(""),
				//[DynamoDbKeys.LastIdempotencyResponse] = new AttributeValue(""),
			},
			UpdateExpression = "SET UA=:t,AA=:t,LRID=:e,LRES=:e",
			ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
			{
				{ ":t", new AttributeValue { N = createdAt.UtcTicks.ToString() } },
				{ ":e", _emptyValue },

			},
		};

		await _dynamoDbService.UpdateItemAsync(request);
	}
}

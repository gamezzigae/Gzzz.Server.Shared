using Amazon.DynamoDBv2.Model;
using Gzzz.Db.DynamoDb;

namespace Gzzz.Services.Authentication;

public interface IUserAuthenciatedInfoUpdater
{
	Task UpdateAuthenticatedInfoAsync(string userId, DateTimeOffset createdAt);
}
public class DefaultTokenUpdateService : IUserAuthenciatedInfoUpdater
{
	Task IUserAuthenciatedInfoUpdater.UpdateAuthenticatedInfoAsync(string userId, DateTimeOffset createdAt) => Task.CompletedTask;
}

public class DynamoDbAuthenciatedInfoUpdater : IUserAuthenciatedInfoUpdater
{
	readonly DynamoDbService _dynamoDbService;
	readonly UpdateItemRequest _cachedRequest;
	readonly AttributeValue _cachedUserId, _cachedTime;
	public DynamoDbAuthenciatedInfoUpdater(DynamoDbService dynamoDbService)
	{
		_dynamoDbService = dynamoDbService;
		_cachedUserId = new();
		_cachedTime = new();
		_cachedRequest = new UpdateItemRequest
		{
			TableName = _dynamoDbService.TableName,
			Key = new Dictionary<string, AttributeValue>
			{
				[DynamoDbKeys.PartitionKey] = new AttributeValue("User"),
				[DynamoDbKeys.SortKey] = _cachedUserId
			},
			UpdateExpression = "SET UA=:t,AA=:t",
			ExpressionAttributeValues = new Dictionary<string, AttributeValue>() { { ":t", _cachedTime } },
		};
	}

	public async Task UpdateAuthenticatedInfoAsync(string userId, DateTimeOffset createdAt)
	{
		_cachedUserId.S = userId;
		_cachedTime.N = createdAt.Ticks.ToString();

		await _dynamoDbService.Client.UpdateItemAsync(_cachedRequest);
	}
}

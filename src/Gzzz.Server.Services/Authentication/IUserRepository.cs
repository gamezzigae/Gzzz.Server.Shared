using Amazon.DynamoDBv2.Model;
using Gzzz.Db.DynamoDb;
namespace Gzzz.Services.Authentication;
public interface IUserRepository
{
	public Dictionary<string, AttributeValue> AttributeMap { get; }
	Task<bool> LoadAsync(string userId, DateTimeOffset authenticatedAt);
}
public class DefaultUserRepository : IUserRepository
{
	public Dictionary<string, AttributeValue> AttributeMap { get; }
	public Task<bool> LoadAsync(string userId, DateTimeOffset authenticatedAt) => Task.FromResult(true);
}

public class DynamoDbUserRepositoryBase : IUserRepository
{
	readonly DynamoDbService _dynamoDbService;
	public Dictionary<string, AttributeValue> AttributeMap { get; private set; }
	public DynamoDbUserRepositoryBase(DynamoDbService dynamoDbService)
	{
		_dynamoDbService = dynamoDbService;
	}
	public async Task<bool> LoadAsync(string userId, DateTimeOffset authenticatedAt)
	{
		this.AttributeMap = await _dynamoDbService.GetAsync(DynamoDbTable.User, userId);
		return authenticatedAt.Ticks.ToString() == AttributeMap[DynamoDbKeys.AuthenticatedAt].N;
	}
}

using Amazon.Runtime;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class DynamoDbFixture : IAsyncLifetime
{
	static readonly AWSCredentials _awsCredentials = new BasicAWSCredentials("DUMMYACCESSKEYDUMMYY", "44nPdvh6gW+EXjh1P6jLXFzmmp4K2F1dUSQx7R4+");
	readonly DynamoDbConfig _dynamoDbConfig = new DynamoDbConfig() { TableName = RandomX.GetRandomText(), ServiceURL = "http://localhost:8000" };
	protected readonly DynamoDbService _dynamoDbService;
	public DynamoDbFixture()
	{
		_dynamoDbService = new DynamoDbService(_awsCredentials, _dynamoDbConfig, null);
	}
	public async ValueTask InitializeAsync()
	{
		await DynamoDbTestUtil.CreateTableAsync(_dynamoDbService, _dynamoDbConfig);
	}
	public async ValueTask DisposeAsync()
	{
		await DynamoDbTestUtil.DeleteTableAsync(_dynamoDbService, _dynamoDbConfig);
	}
}

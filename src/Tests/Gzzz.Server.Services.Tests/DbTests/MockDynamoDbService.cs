using Amazon.Runtime;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class MockDynamoDbService : DynamoDbService
{
	static readonly AWSCredentials _awsCredentials = new BasicAWSCredentials("DUMMYACCESSKEYDUMMYY", "44nPdvh6gW+EXjh1P6jLXFzmmp4K2F1dUSQx7R4+");

	public MockDynamoDbService() : base(_awsCredentials, new DynamoDbConfig() { TableName = RandomX.GetRandomText(), ServiceURL = "http://localhost:8000" })
	{
	}
	public async Task CreateTableAsync()
	{
		await DynamoDbTestUtil.CreateTableAsync(this, _dynamoDbConfig);
	}

	public async Task DeleteTableAsync()
	{
		await DynamoDbTestUtil.DeleteTableAsync(this, _dynamoDbConfig);
	}
}

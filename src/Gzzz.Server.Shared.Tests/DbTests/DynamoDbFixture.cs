using Amazon.Runtime;

namespace Gzzz.Server.Shared.Tests.Db;

class DynamoDbFixture
{
	protected readonly DynamoDbService _dynamoDbService;
	readonly DynamoDbConfig _dynamoDbConfig;
	public DynamoDbFixture()
	{
		var awsCredentials = new BasicAWSCredentials("DUMMYACCESSKEYDUMMYY", "44nPdvh6gW+EXjh1P6jLXFzmmp4K2F1dUSQx7R4+");
		_dynamoDbConfig = new DynamoDbConfig(RandomX.GetRandomText(), "http://localhost:8000");
		_dynamoDbService = new DynamoDbService(awsCredentials, _dynamoDbConfig);
	}

	[OneTimeSetUp]
	public async Task SetupFixtureAsync()
	{
		await DynamoDbTestUtil.CreateTableAsync(_dynamoDbService, _dynamoDbConfig);
	}

	[OneTimeTearDown]
	public async Task TearDownFixtureAsync()
	{
		await DynamoDbTestUtil.DeleteTableAsync(_dynamoDbService, _dynamoDbConfig);
	}
}

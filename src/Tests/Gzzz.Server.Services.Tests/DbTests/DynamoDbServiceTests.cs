using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon.Runtime;
using Gzzz.Db.DynamoDb;
using Gzzz.Server.Shared.Tests.DbTests;
using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.Server.Shared.Tests.DbTests;

public class DynamoDbFixture : IAsyncLifetime
{
	static readonly AWSCredentials _awsCredentials = new BasicAWSCredentials("DUMMYACCESSKEYDUMMYY", "44nPdvh6gW+EXjh1P6jLXFzmmp4K2F1dUSQx7R4+");
	readonly DynamoDbConfig _dynamoDbConfig = new DynamoDbConfig() { TableName = RandomX.GetRandomText(), ServiceURL = "http://localhost:8000" };
	protected readonly DynamoDbService _dynamoDbService;
	public DynamoDbFixture()
	{
		_dynamoDbService = new DynamoDbService(_awsCredentials, _dynamoDbConfig);
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

public class DynamoDbServiceTests : DynamoDbFixture
{
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow.TrimBelowMilliseconds();

	[Fact]
    public async Task InsertItemTestAsync()
    {
        var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
        var attributeMap = AttributeMap.ConvertFrom(item);
        await _dynamoDbService.PutItemAsync(attributeMap, _now);

        var retrievedItem = await _dynamoDbService.GetAttirubtesAsync(item.PK, item.SK);
		AssertX.JsonEquals(attributeMap["PK"], retrievedItem["PK"]);
		AssertX.JsonEquals(attributeMap["SK"], retrievedItem["SK"]);
		AssertX.JsonEquals(attributeMap["Age"], retrievedItem["Age"]);

		Assert.Equal( retrievedItem.GetUpdatedAt(), (_now));

	}

	[Fact]
	public async Task InsertExistsKeyErrorTestAsync()
	{
		var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
		var attributeMap = AttributeMap.ConvertFrom(item);
		await _dynamoDbService.PutItemAsync(attributeMap, _now);
		await Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _dynamoDbService.PutItemAsync(attributeMap, _now));
	}

	[Fact]
	public async Task UpdateItemTestAsync()
	{
		var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
		var attributeMap = AttributeMap.ConvertFrom(item);
		await _dynamoDbService.PutItemAsync(attributeMap, _now);
			
		var updateItem = new { item.PK, item.SK, Age = item.Age + 1 };
		var updateAttributeMap = AttributeMap.ConvertFrom(updateItem);
		await _dynamoDbService.PutItemAsync(updateAttributeMap, _now.AddMilliseconds(1), _now);

		var retrievedItem = await _dynamoDbService.GetAttirubtesAsync(item.PK, item.SK);
		AssertX.JsonEquals(updateAttributeMap["PK"], retrievedItem["PK"]);
		AssertX.JsonEquals(updateAttributeMap["SK"], retrievedItem["SK"]);
		AssertX.JsonEquals(updateAttributeMap["Age"], retrievedItem["Age"]);
	}

	[Fact]
	public async Task UpdateTimestampTestAsync()
	{
		var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
		var attributeMap = AttributeMap.ConvertFrom(item);
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dynamoDbService.PutItemAsync(attributeMap, _now, _now));
		Assert.Equal("dynamodb putitem time condition error",exception.Message);
	}
}

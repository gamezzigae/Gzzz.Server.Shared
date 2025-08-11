using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Gzzz.Server.Shared.Tests.DbTests;

namespace Gzzz.Server.Shared.Tests.Db;

class DynamoDbServiceTests
{
	MockDynamoDbService _dynamoDbService = new MockDynamoDbService();
	[OneTimeSetUp]
	public async Task OneTimeSetUpAsync()
	{
		await _dynamoDbService.CreateTableAsync();
	}
	[OneTimeTearDown]
	public async Task OneTimeTearDownAsync()
	{
		await _dynamoDbService.DeleteTableAsync();
	}
	[Test]
    public async Task InsertItemTestAsync()
    {
        var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), TS= RandomX.GetRandom(), Age = RandomX.GetRandom() };
        var attributeMap = AttributeMap.ConvertFrom(item);
        await _dynamoDbService.PutItemAsync(attributeMap);

        var retrievedItem = await _dynamoDbService.GetAttirubtesAsync(item.PK, item.SK);
        AssertX.JsonEquals(attributeMap, retrievedItem);
    }

    [Test]
    public async Task InsertExistsKeyErrorTestAsync()
    {
        var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), TS = RandomX.GetRandom(), Age = RandomX.GetRandom() };
        var attributeMap = AttributeMap.ConvertFrom(item);
        await _dynamoDbService.PutItemAsync(attributeMap);
        Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _dynamoDbService.PutItemAsync(attributeMap));
    }

    [Test]
    public async Task UpdateItemTestAsync()
    {
        var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), TS = RandomX.GetRandom(), Age = RandomX.GetRandom() };
        var attributeMap = AttributeMap.ConvertFrom(item);
        await _dynamoDbService.PutItemAsync(attributeMap);

        var updateItem = new { item.PK, item.SK, TS = RandomX.GetRandom(), Age = item.Age + 1 };
        var updateAttributeMap = AttributeMap.ConvertFrom(updateItem);
        await _dynamoDbService.PutItemAsync(updateAttributeMap, AttributeMap.CreateTimestamp(item.TS));

        var retrievedItem = await _dynamoDbService.GetAttirubtesAsync(item.PK, item.SK);
        AssertX.JsonEquals(updateAttributeMap, retrievedItem);
    }

    [Test]
    public void UpdateTimestampTestAsync()
    {
        var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), TS = RandomX.GetRandom(), Age = RandomX.GetRandom() };
        var attributeMap = AttributeMap.ConvertFrom(item);
        var exception = Assert.ThrowsAsync<ArgumentException>(() => _dynamoDbService.PutItemAsync(attributeMap, attributeMap["TS"]));
        Assert.That(exception.Message, Is.EqualTo("before/after timestamp is equals"));
    }
}

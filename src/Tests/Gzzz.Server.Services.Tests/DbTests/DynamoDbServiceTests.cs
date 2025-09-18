using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Gzzz.Server.Shared.Tests.DbTests;

namespace Gzzz.Server.Shared.Tests.Db;

class DynamoDbServiceTests
{
	readonly MockDynamoDbService _dynamoDbService = new MockDynamoDbService();
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow.TrimBelowMilliseconds();
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
        var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
        var attributeMap = AttributeMap.ConvertFrom(item);
        await _dynamoDbService.PutItemAsync(attributeMap, _now);

        var retrievedItem = await _dynamoDbService.GetAttirubtesAsync(item.PK, item.SK);
		AssertX.JsonEquals(attributeMap["PK"], retrievedItem["PK"]);
		AssertX.JsonEquals(attributeMap["SK"], retrievedItem["SK"]);
		AssertX.JsonEquals(attributeMap["Age"], retrievedItem["Age"]);

		Assert.That( retrievedItem.GetUpdatedAt(), Is.EqualTo(_now));

	}

	[Test]
	public async Task InsertExistsKeyErrorTestAsync()
	{
		var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
		var attributeMap = AttributeMap.ConvertFrom(item);
		await _dynamoDbService.PutItemAsync(attributeMap, _now);
		Assert.ThrowsAsync<ConditionalCheckFailedException>(() => _dynamoDbService.PutItemAsync(attributeMap, _now));
	}

	[Test]
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

	[Test]
	public void UpdateTimestampTestAsync()
	{
		var item = new { PK = RandomX.GetRandomText(), SK = RandomX.GetRandomText(), Age = RandomX.GetRandom() };
		var attributeMap = AttributeMap.ConvertFrom(item);
		var exception = Assert.ThrowsAsync<ArgumentException>(() => _dynamoDbService.PutItemAsync(attributeMap, _now, _now));
		Assert.That(exception.Message, Is.EqualTo("dynamodb putitem time condition error"));
	}
}

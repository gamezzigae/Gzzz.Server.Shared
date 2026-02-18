using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;

namespace Gzzz.Db.DynamoDb;

public static class DynamoDbTestUtil
{
    public static async Task CreateTableAsync(DynamoDbService db, DynamoDbConfig dynamoDbConfig)
    {
        if (dynamoDbConfig.ServiceURL == default)
            throw new Exception("테스트용 ServiceURL이 없습니다.");
        var tableName = db.TableName;
		var dynamodb = db.Client;
		_ = await dynamodb.CreateTableAsync(
			new CreateTableRequest()
			{
				TableName = tableName,
				KeySchema = [
					new KeySchemaElement(DynamoDbKeys.PartitionKey, KeyType.HASH),
					new KeySchemaElement(DynamoDbKeys.SortKey, KeyType.RANGE)
				],
				AttributeDefinitions = [
					new AttributeDefinition(DynamoDbKeys.PartitionKey, ScalarAttributeType.S),
					new AttributeDefinition(DynamoDbKeys.SortKey, ScalarAttributeType.S)
				],
				ProvisionedThroughput = new ProvisionedThroughput(1, 1)
			});

		await WaitTableCreation(dynamodb, tableName);
	}

    public static async Task WaitTableCreation(AmazonDynamoDBClient dynamodb, string tableName)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var describeResponse = await dynamodb.DescribeTableAsync(tableName);
                return;
            }
            catch (ResourceNotFoundException nfex) when (nfex.Message == "Cannot do operations on a non-existent table")
            {
                await Task.Delay(100);
            }
            catch(Exception)
            {
                throw;
            }
        }
    }


    public static async Task DeleteTableAsync(DynamoDbService db, DynamoDbConfig dynamoDbConfig)
    {
        if (dynamoDbConfig.ServiceURL == default)
            throw new Exception("테스트용 ServiceURL이 없습니다.");
        var tableName = db.TableName;
        var dynamodb = db.Client;
        try
        {
            _ = await dynamodb.DeleteTableAsync(tableName);
        }
        catch (Exception)
        {
        }
    }
}

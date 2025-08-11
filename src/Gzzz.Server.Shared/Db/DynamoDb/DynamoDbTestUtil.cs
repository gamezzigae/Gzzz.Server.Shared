using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.Db.DynamoDb;

public static class DynamoDbTestUtil
{
    public static async Task CreateTableAsync(DynamoDbService db, DynamoDbConfig dynamoDbConfig)
    {
        if (dynamoDbConfig.ServiceURL == default)
            throw new Exception("테스트용 ServiceURL이 없습니다.");
        var tableName = db.TableName;
		var dynamodb = db.GetClient();
		_ = await dynamodb.CreateTableAsync(
			new CreateTableRequest()
			{
				TableName = tableName,
				KeySchema = [
					new KeySchemaElement("PK", KeyType.HASH),
					new KeySchemaElement("SK", KeyType.RANGE)
				],
				AttributeDefinitions = [
					new AttributeDefinition("PK", ScalarAttributeType.S),
					new AttributeDefinition("SK", ScalarAttributeType.S)
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
        var dynamodb = db.GetClient();
        try
        {
            _ = await dynamodb.DeleteTableAsync(tableName);
        }
        catch (Exception)
        {
        }
    }
}

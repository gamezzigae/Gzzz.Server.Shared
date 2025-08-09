using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.DynamoDb;

public static class DynamoDbTestUtil
{
	public static IServiceCollection UseMockDynamoDb(this IServiceCollection services, string tableName) =>
		services.AddSingleton<AWSCredentials>(new BasicAWSCredentials("DUMMYACCESSKEYDUMMYY", "44nPdvh6gW+EXjh1P6jLXFzmmp4K2F1dUSQx7R4+"))
				.AddTransient<AmazonDynamoDBClient>()
				.AddSingleton<DynamoDbService>()
				.AddSingleton<DynamoDbConfiguration>(new DynamoDbConfiguration(tableName));

	public static async Task CreateTableAsync(DynamoDbService db)
	{
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


	public static Task CreateTableAsync(IServiceProvider services) => CreateTableAsync(services.GetRequiredService<DynamoDbService>());
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


    public static async Task DeleteTableAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<DynamoDbService>();
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

namespace Gzzz.Server.Shared.Tests.DbTests;

class TestDynamoDbRepository : DynamoDbOptimisticRepository<TestEntity>
{
	public TestDynamoDbRepository(DynamoDbService dynamoDbService) : base(dynamoDbService, RandomX.GetRandomText())
	{
	}
}

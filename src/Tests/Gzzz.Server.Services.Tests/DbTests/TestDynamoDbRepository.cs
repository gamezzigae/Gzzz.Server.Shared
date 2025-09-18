namespace Gzzz.Server.Shared.Tests.DbTests;

class TestDynamoDbRepository : DynamoDbOptimisicRepository<TestEntity>
{
	public TestDynamoDbRepository(DynamoDbService dynamoDbService) : base(dynamoDbService, RandomX.GetRandomText())
	{
	}
}

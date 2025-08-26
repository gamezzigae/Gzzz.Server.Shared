namespace Gzzz.Server.Shared.Tests.DbTests;

public class TestEntity
{
	public string UserId { get; set; }
	public int Level { get; set; }

	public static TestEntity CreateRandom() => new TestEntity
	{
		UserId = RandomX.GetRandomText(),
		Level = RandomX.GetRandom()
	};
}

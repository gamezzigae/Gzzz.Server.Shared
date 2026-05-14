namespace Gzzz.CommandInvoker.Tests;

public class IdempotencyTest : AwsFunctionUrlInvokerFixture
{
	public IdempotencyTest(ITestOutputHelper testLogger) : base(testLogger)
	{
	}

	[Fact]
	public async Task IdempotencyTestAsync()
	{
		var client = await CreateSignedClientAsync(RandomX.GetRandomText());
		var response = await client.RequestAsync<string>("/test/idempotency", ApiOption.Idempotency);
	}
}

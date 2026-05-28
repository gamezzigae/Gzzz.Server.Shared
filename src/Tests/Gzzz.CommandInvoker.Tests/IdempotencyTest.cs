namespace Gzzz.CommandInvoker.Tests;

public class IdempotencyTest : AwsFunctionUrlInvokerFixture
{
	public IdempotencyTest(ITestOutputHelper testLogger) : base(testLogger)
	{
	}

	[Fact]
	public async Task IdempotencyTest1Async()
	{
		var client = await CreateSignedClientAsync(RandomX.GetRandomText());
		var response1 = await client.RequestAsync<string>("/test/idempotency", ApiOption.Idempotency);
		var response2 = await client.RequestAsync<string>("/test/idempotency", ApiOption.Idempotency);
		var response3 = await client.RequestAsync<string>("/test/idempotency", ApiOption.Idempotency);

		Assert.Equal(response1, response2);
		Assert.Equal(response1, response3);
	}
	[Fact]
	public async Task IdempotencyTest2Async()
	{
		var client = await CreateSignedClientAsync(RandomX.GetRandomText());
		var response1 = await client.RequestAsync<string>("/test/idempotency", ApiOption.Idempotency);
		client.NextRequestId();
		var response2 = await client.RequestAsync<string>("/test/idempotency", ApiOption.Idempotency);

		Assert.NotEqual(response1, response2);
	}
}

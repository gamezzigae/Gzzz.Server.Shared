namespace Gzzz.CommandInvoker.Tests;

[Controller("test")]
public class TestController
{
	readonly RequestInfo _requestInfo;

	public TestController(RequestInfo requestInfo)
	{
		_requestInfo = requestInfo;
	}
	[Command("/echo")]
	public Task<string> GetStringAsync(string message) => Task.FromResult(message);
	[Command("/servicefirst")]
	public Task<string> GetServiceFirstAsync([FromService] RequestInfo _, string message) => Task.FromResult(message);
	[AnonymousCommand("/hello")]
	public Task<string> GetStringAsync() => Task.FromResult("world");
	[Command("/nothing")]
	public Task NothingAsync() => Task.CompletedTask;

	[AnonymousCommand("/echodatetime")]
	public Task<DateTime> EchoTime(DateTime datetime) => Task.FromResult(datetime);

	[AnonymousCommand("/requestinfo")]
	public Task GetRequestInfoAsync() => Task.CompletedTask;

	[IdempotencyCommand("/idempotency")]
	public Task IdempotencyTestAsync()=>Task.FromResult(RandomX.GetRandomText());
}

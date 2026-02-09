using Gzzz;
using Gzzz.CommandInvoker;
using Microsoft.Extensions.DependencyInjection;

namespace Project1.Controllers;


[Controller]
public class TestController
{
	[AnonymousCommand("/echo")]
	public Task<string> EchoAsync(string input) => Task.FromResult("echo:" + input);

	[AnonymousCommand("/int")]
	public Task<int> GetUserInfoAsync() => Task.FromResult(Random.Shared.Next());

}

public class PutItemRequest
{
	public string PK { get; set; }
	public string SK { get; set; }
	public string UA { get; set; }
	public string Value { get; set; }
}

[Controller]
public class UserController
{
	[Command("/exception")]
	public Task ExceptionAsync() => throw new HttpException(555, "Test exception from UserController.GetUserInfoAsync()");
	[Command("/unhandledexception")]
	public Task UnhandledExceptionAsync() => throw new Exception("Test exception from UserController.GetUserInfoAsync()");
}

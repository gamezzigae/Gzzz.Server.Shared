using Gzzz;
using Gzzz.CommandInvoker;
using Gzzz.Services.Authentication;
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

[Controller]
public class UserController
{
	[Command("/exception")]
	public Task ExceptionAsync() => throw new HttpException(555, "Test exception from UserController.GetUserInfoAsync()");
	[Command("/unhandledexception")]
	public Task UnhandledExceptionAsync() => throw new Exception("Test exception from UserController.GetUserInfoAsync()");
}

[Controller]
public class SaveLoadController
{
	readonly IUserRepository _userRepository;

	public SaveLoadController(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	[UpdateCommand("/save")]
	public async Task SaveAsync(string content)
	{
		_userRepository.AttributeMap["content"] = new () { S = content };
	}
	[Command("/load")]
	public async Task<string> LoadAsync()
	{
		return _userRepository.AttributeMap["content"].S;
	}
}

using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.CommandInvoker.Tests;

public class Tests
{
	ServiceProvider Setup(string path, out CommandInfo command)
	{

		var services = new ServiceCollection()
			.AddCommandInvokers(typeof(TestController).Assembly)
			.BuildServiceProvider();
		command = services.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>()[path];
		return services; ;
	}

	[Test]
	public async Task EchoTestAsync()
	{
		using var services = Setup("/test/echo", out var command);
		var message = "Hello, World!";
		var echo = await command.InvokeAsync<string>(services, message);
		Assert.That(echo, Is.EqualTo(message));
	}

	[Test]
	public async Task NoParameterTestAsync()
	{
		using var services = Setup("/test/hello", out var command);
		var echo = await command.InvokeAsync<string>(services, null);
		Assert.That(echo, Is.EqualTo("world"));
	}
	[Test]
	public async Task NoParameterNoReturnTestAsync()
	{
		using var services = Setup("/test/nothing", out var command);
		var echo = await command.InvokeAsync<string>(services, null);
		Assert.That(echo, Is.Null);
	}
}

[Controller("test")]
public class TestController
{
	[Command("/echo")]
	public Task<string> GetStringAsync(string message) => Task.FromResult(message);
	[Command("/hello")]
	public Task<string> GetStringAsync() => Task.FromResult("world");
	[Command("/nothing")]
	public Task NothingAsync() => Task.CompletedTask;
}

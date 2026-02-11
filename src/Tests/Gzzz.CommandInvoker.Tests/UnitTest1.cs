using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.CommandInvoker.Tests;

public class Tests
{
	ServiceProvider Setup(string path, out CommandInfo command)
	{

		var services = new ServiceCollection()
			.AddCommandInvokers(typeof(TestController).Assembly)
			.AddScoped<RequestInfo>()
			.BuildServiceProvider();
		command = services.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>()[path];
		return services; ;
	}

	[Fact]
	public async Task EchoTestAsync()
	{
		using var services = Setup("/test/echo", out var command);
		var message = "Hello, World!";
		var echo = await command.InvokeAsync<string>(services, message);
		Assert.Equal(message, echo);
	}

	[Fact]
	public async Task NoParameterTestAsync()
	{
		using var services = Setup("/test/hello", out var command);
		var echo = await command.InvokeAsync<string>(services, null);
		Assert.Equal("world", echo);
	}
	[Fact]
	public async Task NoParameterNoReturnTestAsync()
	{
		using var services = Setup("/test/nothing", out var command);
		var echo = await command.InvokeAsync<string>(services, null);
		Assert.Null(echo);
	}
}

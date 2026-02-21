using Gzzz.CommandInvoker;
using System.Reflection;

public class CommandInvokerBenchmarkBase
{
	protected readonly TestController _controller = new TestController ();
	protected readonly CommandInfo _commandInfo;
	protected readonly IServiceProvider _serivces;
	public CommandInvokerBenchmarkBase()
	{
		Assembly[] assem = [typeof(TestController).Assembly];

		_serivces = new ServiceCollection()
				.AddCommandInvokers(assem)
				.BuildWithValidation();

		var commands = _serivces.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>();
		_commandInfo= commands["/test"];
	}

	[Controller(serviceLifetime: ServiceLifetime.Singleton)]
	public class TestController
	{
		[Command("/test")]
		public Task TestAsync(object _) => Task.CompletedTask;
	}


}

using Gzzz.CommandInvoker;
using System.Reflection;

public class CommandInvokerBenchmarkBase
{
	protected readonly TestController _controller = new TestController ();
	protected readonly IServiceProvider _serivces;
	protected readonly IReadOnlyDictionary<string, CommandInfo> _commands;

	public virtual void ConfigureServices(IServiceCollection services) { }

	public CommandInvokerBenchmarkBase()
	{
		Assembly[] assem = [typeof(CommandInvokerBenchmarkBase).Assembly];

		var services = new ServiceCollection()
				.AddCommandInvokers(assem)
				.AddTransient<ApiContext>();

		ConfigureServices(services);

		_serivces = services.BuildWithValidation();

		_commands = _serivces.GetRequiredService<IReadOnlyDictionary<string, CommandInfo>>();
	}

	[Controller(serviceLifetime: ServiceLifetime.Singleton)]
	public class TestController
	{
		[Command("/test")]
		public Task TestAsync(object _) => Task.CompletedTask;
	}


}

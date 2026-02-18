using Amazon.DynamoDBv2.Model;
using Gzzz.CommandInvoker;
using Gzzz.Controllers;
using Gzzz.Db.DynamoDb;
using System.Reflection;

BenchmarkRunner.Run<UseObjectPoolBenchmark>();

[MemoryDiagnoser]
public class UseObjectPoolBenchmark : CommandInvokerBenchmarkBase
{
	readonly object _parameter = new object();
	[Benchmark]
	public async Task WithObjectPool()
	{
		for(int i = 0; i < 100000; i++)
		{
			await _commandInfo.InvokeAsync(_serivces, _parameter);
		}
	}
	[Benchmark] public async Task NoObjectPool()
	{
		for (int i = 0; i < 100000; i++)
		{
			await _commandInfo.InvokeAsync2(_serivces, _parameter);
		}
	}

}

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

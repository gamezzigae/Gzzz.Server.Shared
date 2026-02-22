using Gzzz.CommandInvoker;

[MemoryDiagnoser]
public class ParameterInfoBenchmark : CommandInvokerBenchmarkBase
{
	public override void ConfigureServices(IServiceCollection services)
	{
		services.AddTransient<ApiContext>();
	}

	[Benchmark]
	public async Task Case1Async()
	{
		for (int i = 0; i < 10000; i++)
			await _commands["/test1"].InvokeAsync(_serivces, "abc");
	}
	[Benchmark]
	public async Task Case2Async()
	{
		for (int i = 0; i < 10000; i++)
			await _commands["/test2"].InvokeAsync(_serivces, "abc");
	}
	[Controller(serviceLifetime: ServiceLifetime.Transient)]
	public class Test1Controller
	{
		readonly ApiContext _apiContext;

		public Test1Controller(ApiContext apiContext)
		{
			_apiContext = apiContext;
		}
		[Command("/test1")]
		public Task TestAsync(string body) => Task.CompletedTask;
	}


	[Controller(serviceLifetime: ServiceLifetime.Transient)]
	public class Test2Controller
	{
		[Command("/test2")]
		public Task TestAsync(string body, [FromService] ApiContext apiContext) => Task.CompletedTask;
	}

	/*
	 * 
	 * 
| Method     | Mean     | Error    | StdDev   | Median   | Gen0     | Allocated |
|----------- |---------:|---------:|---------:|---------:|---------:|----------:|
| Case0Async | 386.9 us |  7.62 us | 16.41 us | 386.9 us | 171.8750 |   1.37 MB |
| Case1Async | 394.1 us |  7.84 us | 18.92 us | 385.7 us | 209.9609 |   1.68 MB | FromService 파라미터가 없으면 확실히 빠르다
| Case2Async | 608.9 us | 11.86 us | 19.82 us | 606.9 us | 219.7266 |   1.75 MB | FromService 파라미터가 있으면 확실히 느려진다

	 * */
}

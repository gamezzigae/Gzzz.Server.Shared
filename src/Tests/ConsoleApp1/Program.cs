using Amazon.DynamoDBv2.Model;
using Gzzz.Controllers;
using Gzzz.Db.DynamoDb;
using System.Linq.Expressions;
using System.Reflection;



BenchmarkRunner.Run<MemoryStreamGCBenchmark>();


[MemoryDiagnoser]
public class MemoryStreamGCBenchmark : CommandInvokerBenchmarkBase
{
	public override void ConfigureServices(IServiceCollection services)
	{
		services.AddTransient<ApiContext>();
	}

	[Benchmark]
	public void Case1()
	{
		new MemoryStream();
	}

	[Benchmark]
	public void Case2()
	{
		new MemoryStream().Dispose();
	}
	[Benchmark]
	public void Case3()
	{
		using var ms = new MemoryStream();
	}


}

[MemoryDiagnoser]
public class UseObjectPoolBenchmark : CommandInvokerBenchmarkBase
{
	readonly object _parameter = new object();
	//[Benchmark]
	//public async Task NewArrayParameter()
	//{
	//	for(int i = 0; i < 100000; i++)
	//	{
	//		await _commandInfo.InvokeAsync(_serivces, _parameter);
	//	}
	//}
	//[Benchmark] public async Task CachedArray()
	//{
	//	for (int i = 0; i < 100000; i++)
	//	{
	//		await _commandInfo.InvokeAsync2(_serivces, _parameter);
	//	}
	//}

}

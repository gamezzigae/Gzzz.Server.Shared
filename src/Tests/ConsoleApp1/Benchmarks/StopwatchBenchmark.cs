using BenchmarkDotNet.Attributes;
using System.Diagnostics;

[MemoryDiagnoser]
public class StopwatchBenchmark
{
	DateTimeOffset _time1;
	DateTime _time2;
	public StopwatchBenchmark()
	{
		_time1 = DateTimeOffset.UtcNow;
		_time2 = DateTime.UtcNow;
		_stopwatch = System.Diagnostics.Stopwatch.StartNew();
	}
	Stopwatch _stopwatch;

	[Benchmark]
	public void StopwatchCase()
	{
		_stopwatch.Reset();
		_stopwatch.Start();
		_ = _stopwatch.ElapsedMilliseconds;
	}
	[Benchmark]
	public void DateTimeCase()
	{
		_= (int)(_time2 - DateTime.UtcNow).TotalMilliseconds;
	}
	[Benchmark]
	public void DateTimeOffsetCase()
	{
		_ = (int)(_time1 - DateTimeOffset.UtcNow).TotalMilliseconds;
	}
}

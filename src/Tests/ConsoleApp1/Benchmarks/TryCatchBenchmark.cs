using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Gzzz;
//BenchmarkRunner.Run<TryCatchBenchmark>();
//
//
//
//


[MemoryDiagnoser]
public class TryCatchBenchmark
{
	string _message = RandomX.CreateRandomBase64String(256);

	[Benchmark]
	public void SerializeToUtf8Bytes()
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(_message);
		using var ms = new MemoryStream(bytes);
	}


	[Benchmark]
	public async Task SerializeStreamAsync()
	{
		using var ms = new MemoryStream();
		await JsonSerializer.SerializeAsync(ms, _message);
	}


	[Benchmark]
	public void SerializeStream()
	{
		using var ms = new MemoryStream();
		JsonSerializer.Serialize(ms, _message);
	}

}

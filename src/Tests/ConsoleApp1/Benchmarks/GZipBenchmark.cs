using BenchmarkDotNet.Attributes;
using Gzzz;
using System.IO.Compression;
using System.Text.Json;

[MemoryDiagnoser]
public class GZipBenchmark
{
	[Params(128, 512, 2048)]
	public int Length { get; set; }

	public TestClass _instance;

	public static long BinarySize { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_instance = new TestClass() { Name = RandomX.GetRandomText(Length) };
	}

	[Benchmark]
	public async Task GZipJson1()
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(_instance);
		using var stream2 = new MemoryStream();
		using (var gzipStream = new GZipStream(stream2, CompressionLevel.Optimal, leaveOpen: true))
		{
			await gzipStream.WriteAsync(bytes);
		}
	}
	[Benchmark]
	public void GZipJson2()
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(_instance);
		using var stream2 = new MemoryStream();
		using (var gzipStream = new GZipStream(stream2, CompressionLevel.Optimal, leaveOpen: true))
		{
			gzipStream.Write(bytes);
		}
	}
	[Benchmark]
	public async Task GZipJson3()
	{
		using var stream2 = new MemoryStream();
		await JsonSerializer.SerializeAsync(stream2, _instance);
		stream2.Position = 0;
		using var stream3 = new MemoryStream();
		using (var gzipStream = new GZipStream(stream3, CompressionLevel.Optimal, leaveOpen: true))
		{
			stream2.CopyTo(gzipStream);
		}


	}
	[Benchmark]
	public async Task GZipJson4()
	{
		using var stream2 = new MemoryStream();
		await JsonSerializer.SerializeAsync(stream2, _instance);
		stream2.Position = 0;
		using var stream3 = new MemoryStream();
		using (var gzipStream = new GZipStream(stream3, CompressionLevel.Optimal, leaveOpen: true))
		{
			await stream2.CopyToAsync(gzipStream);
		}


	}

	//[Benchmark]
	//public async Task Json()
	//{
	//	using var stream2 = new MemoryStream();
	//	await JsonSerializer.SerializeAsync(stream2, _instance);

	//	BinarySize= stream2.Position;
	//}

	public class TestClass
	{
		public string Name { get; set; }
	}
}

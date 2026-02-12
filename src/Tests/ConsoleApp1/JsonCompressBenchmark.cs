using BenchmarkDotNet.Attributes;
using Gzzz.Serialize;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ConsoleApp1;

[MemoryDiagnoser]
public class JsonCompressBenchmark
{
	Dictionary<string, int> _items = new Dictionary<string, int>();
	public JsonCompressBenchmark()
	{
		for (int i = 0; i < 3000; i++)
		{
			_items.Add("key" + i, i);
		}
	}




	[Benchmark]
	public int Case1()
	{
		var json = Json.Serialize(_items);
		var bytes = DefaultConfig.Encoding.GetBytes(json);
		var compressed = Zstd.Compress(bytes);
		return compressed.Length;
	}


	[Benchmark]
	public async Task<int> Case2()
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(_items);
		using var memoryStream = new MemoryStream();
		using var zstdStream = new ZstdSharp.CompressionStream(memoryStream);

		zstdStream.Write(bytes, 0, bytes.Length);
		zstdStream.Flush();

		var compressed = memoryStream.ToArray();
		return compressed.Length;
	}

}

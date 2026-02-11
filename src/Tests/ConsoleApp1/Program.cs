using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ConsoleApp1;
using Gzzz;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

Bar bar = new Bar { Name = "John", Email = "test@test.com" };
Foo foo = bar;

string json = JsonSerializer.Serialize(foo, typeof(Foo));
Console.WriteLine(json);

BenchmarkRunner.Run<JsonWriterBenchmark>();


public class Foo
{
	public string Name { get; set; }
}

public class Bar : Foo
{
	[JsonPropertyName("email_address")]
	public string Email { get; set; }
}

[MemoryDiagnoser]
public class DictionaryBenchmark
{
	readonly Dictionary<int, int> _dictionary = new Dictionary<int, int>()
	{
		{1, 1}, {2, 2}, {3,3 }
	};
	public DictionaryBenchmark()
	{

	}

	[Benchmark]
	public void Copy()
	{
		var dic = new Dictionary<int, int>(_dictionary);
		dic.Add(4, 4);
	}


	[Benchmark]
	public void AddRemove()
	{
		_dictionary.Add(4, 4);
		_dictionary.Remove(4);
	}
}

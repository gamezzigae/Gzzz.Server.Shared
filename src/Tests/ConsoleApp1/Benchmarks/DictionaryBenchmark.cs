using BenchmarkDotNet.Attributes;

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

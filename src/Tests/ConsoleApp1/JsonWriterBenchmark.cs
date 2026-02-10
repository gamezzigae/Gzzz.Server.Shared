using BenchmarkDotNet.Attributes;
using Gzzz.Serialize;
using System.Buffers;
using System.Text.Json;

[MemoryDiagnoser]
public class JsonWriterBenchmark
{
	Version _item = new Version(1, 2, 3, 4);
	int _id => _item.Major;

	[Benchmark]
	public void Case2()
	{
		_ = Json.Write(_item.Major, static (writer, id) =>
		{
			writer.WriteNumber("Id", id);
			writer.WriteString("Name", "Test");
			for (int i = 0; i < 10; i++)
			{
				writer.WriteNumber("Value" + i, i);
			}
		});
	}

	[Benchmark]
	public void Case3()
	{
		_ = Json.Write(_item, static (writer, item) =>
		{
			writer.WriteNumber("Id", item.Major);
			writer.WriteString("Name", "Test");
			for (int i = 0; i < 10; i++)
			{
				writer.WriteNumber("Value" + i, i);
			}
		});
	}


	[Benchmark]
	public void AddRemove()
	{
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter writer = new Utf8JsonWriter(buffer);
		writer.WriteStartObject();
		writer.WriteNumber("Id", _id);
		writer.WriteString("Name", "Test");
		for (int i = 0; i < 10; i++)
		{
			writer.WriteNumber("Value" + i, i);
		}
		writer.WriteEndObject();
		writer.Flush();
		_ = DefaultConfig.Encoding.GetString(buffer.WrittenSpan);
	}
}

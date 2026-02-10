using BenchmarkDotNet.Attributes;
using Gzzz.Serialize;
using System.Buffers;
using System.Text.Json;

[MemoryDiagnoser]
public class JsonWriterBenchmark
{

	[Benchmark]
	public void Case1()
	{
		int id = 1;
		_ = Json.Write(writer =>
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
	public void AddRemove()
	{
		int id = 1;
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter writer = new Utf8JsonWriter(buffer);
		writer.WriteStartObject();
		writer.WriteNumber("Id", id);
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

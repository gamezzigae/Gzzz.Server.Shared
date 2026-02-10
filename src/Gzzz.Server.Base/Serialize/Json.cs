using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Gzzz.Serialize;

public static class DefaultConfig
{
    public static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
    public static readonly Encoding Encoding = new UTF8Encoding(false);
}

public static class Json
{

	public static void InitializeSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
	{
		if (_options != default)
			throw new InvalidOperationException("JsonSerializerOptions는 한번만 초기화 가능합니다");

		_options = jsonSerializerOptions;
	}

	static JsonSerializerOptions _options = default;

	public static string Serialize(object item)=> JsonSerializer.Serialize(item, _options);
	public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
	public static object Deserialize(string json, Type type) => JsonSerializer.Deserialize(json, type, _options);


	public static string Write(Action<Utf8JsonWriter> write)
	{
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter writer = new Utf8JsonWriter(buffer);

		writer.WriteStartObject();
		write(writer);
		writer.WriteEndObject();
		writer.Flush();
		string json = DefaultConfig.Encoding.GetString(buffer.WrittenSpan);

		return json;
	}
	public static JsonDocument WriteDocument(Action<Utf8JsonWriter> write)
	{
		string json = Write(write);
		return JsonDocument.Parse(json);
	}
}

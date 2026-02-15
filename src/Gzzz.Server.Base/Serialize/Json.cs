using System.Buffers;
using System.Formats.Asn1;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gzzz.Serialize;

public static class DefaultConfig
{
    public static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
    public static readonly Encoding Encoding = new UTF8Encoding(false);


	public static JsonSerializerOptions JsonSerializerOptions { get; private set; }
	public static void Initialize(JsonSerializerOptions jsonSerializerOptions)
	{
		if (JsonSerializerOptions != default)
			throw new InvalidOperationException("JsonSerializerOptions는 한번만 초기화 가능합니다");

		jsonSerializerOptions.Converters.Add(new MemoryStreamConverter());

		JsonSerializerOptions = jsonSerializerOptions;
	}

}

public static class Json
{
	public static string Serialize(object item) => JsonSerializer.Serialize(item, DefaultConfig.JsonSerializerOptions);
	public static byte[] SerializeBytes(object item) => JsonSerializer.SerializeToUtf8Bytes(item, DefaultConfig.JsonSerializerOptions);
	public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, DefaultConfig.JsonSerializerOptions);
}

public static class JsonWriter
{
	public static string Write<TState>(TState state, Action<Utf8JsonWriter, TState> action)
	{
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter writer = new Utf8JsonWriter(buffer);
		writer.WriteStartObject();
		action(writer, state);
		writer.WriteEndObject();
		writer.Flush();
		string json = DefaultConfig.Encoding.GetString(buffer.WrittenSpan);
		return json;
	}

	public static JsonDocument WriteDocument<TState>(TState state, Action<Utf8JsonWriter, TState> action) => JsonDocument.Parse(Write<TState>(state, action));
}

public class MemoryStreamConverter : JsonConverter<MemoryStream>
{
	public override MemoryStream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var bytes = reader.GetBytesFromBase64();
		return new MemoryStream(bytes, writable: false);
	}

	public override void Write(Utf8JsonWriter writer, MemoryStream value, JsonSerializerOptions options)
	{
		if (!value.TryGetBuffer(out var segment))
			segment = new ArraySegment<byte>(value.ToArray());

		writer.WriteBase64StringValue(segment.AsSpan(0, (int)value.Length));
	}
}

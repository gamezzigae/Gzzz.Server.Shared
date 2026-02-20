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
		JsonSerializerOptions = jsonSerializerOptions;
		JsonSerializerOptions.MakeReadOnly();
	}

}

public static class Json
{
	public static string Serialize<T>(T item) => JsonSerializer.Serialize<T>(item, DefaultConfig.JsonSerializerOptions);
	public static byte[] SerializeBytes(object item) => JsonSerializer.SerializeToUtf8Bytes(item, DefaultConfig.JsonSerializerOptions);
	public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, DefaultConfig.JsonSerializerOptions);
	public static string Serialize(object value, Type type) => JsonSerializer.Serialize(value, type, DefaultConfig.JsonSerializerOptions);
	public static object Deserialize(string json, Type type) => JsonSerializer.Deserialize(json, type, DefaultConfig.JsonSerializerOptions);
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

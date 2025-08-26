using System.Text.Json;
namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public class JsonContextSerializer : IContextSerializer
{
	public JsonContextSerializer(JsonSerializerOptions jsonSerializerOptions)
	{
		_jsonSerializerOptions = jsonSerializerOptions;
	}
	readonly JsonSerializerOptions _jsonSerializerOptions;
	public string Serialize(object value) => JsonSerializer.Serialize(value, _jsonSerializerOptions);
	public object Derialize(Type type, string input)=>JsonSerializer.Deserialize(input, type, _jsonSerializerOptions);
}

using Gzzz.Serialize;
using System.Text.Json;
namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public class JsonContextSerializer : IContextSerializer
{
	public string Serialize(object value, Type type) => JsonSerializer.Serialize(value, type, DefaultConfig.JsonSerializerOptions);
	public object Derialize(string json, Type type) => JsonSerializer.Deserialize(json, type, DefaultConfig.JsonSerializerOptions);
}

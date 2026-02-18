using Gzzz.Serialize;
using System.Text.Json;
namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public class JsonContextSerializer : IContextSerializer
{
	public string Serialize(object value, Type type) => Json.Serialize(value, type);
	public object Derialize(string json, Type type) => Json.Deserialize(json, type);
}

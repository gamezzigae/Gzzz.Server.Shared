using Gzzz.Serialize;
namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public class JsonContextSerializer : IContextSerializer
{
	public string Serialize(object value, Type type) => Json.Serialize(value, type);
	public object Derialize(string input, Type type) => Json.Deserialize(input, type);
}

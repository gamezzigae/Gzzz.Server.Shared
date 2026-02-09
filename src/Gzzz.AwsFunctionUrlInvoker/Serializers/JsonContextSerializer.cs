using Gzzz.Serialize;
namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public class JsonContextSerializer : IContextSerializer
{
	public string Serialize(object value) => Json.Serialize(value);
	public object Derialize(Type type, string input)=>Json.Deserialize(input, type);
}

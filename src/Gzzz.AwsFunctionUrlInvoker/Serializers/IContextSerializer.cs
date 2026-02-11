namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public interface IContextSerializer
{
	object Derialize(string input, Type type);
	string Serialize(object value, Type type);
}

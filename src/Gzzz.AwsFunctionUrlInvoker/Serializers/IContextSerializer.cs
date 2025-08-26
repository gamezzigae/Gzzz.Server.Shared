namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

public interface IContextSerializer
{
	object Derialize(Type type, string input);
	string Serialize(object value);
}

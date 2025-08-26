namespace Gzzz.CommandInvoker;



[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
	public CommandAttribute(string path)
	{
		Path = path;
	}

	public string Path { get; }
}


[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AnonymousCommandAttribute : CommandAttribute
{
	public AnonymousCommandAttribute(string path) : base(path)
	{
	}
}


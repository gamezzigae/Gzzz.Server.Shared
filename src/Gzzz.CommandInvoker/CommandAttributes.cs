namespace Gzzz.CommandInvoker;



[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
	public CommandAttribute(string path="", LoggingType loggingType= LoggingType.Detailed)
	{
		this.Path = string.IsNullOrEmpty(path) ? ""
			: path.StartsWith('/') ? path
			: "/"+ path;
		this.LoggingType = loggingType;
	}

	public string Path { get; }
	public LoggingType LoggingType { get; }
}


[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AnonymousCommandAttribute : CommandAttribute
{
	public AnonymousCommandAttribute(string path = "", LoggingType loggingType = LoggingType.Detailed) : base(path, loggingType)
	{
	}
}



[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class IdempotencyCommandAttribute : CommandAttribute
{
	public IdempotencyCommandAttribute(string path = "", LoggingType loggingType = LoggingType.Detailed) : base(path, loggingType)
	{
	}
}

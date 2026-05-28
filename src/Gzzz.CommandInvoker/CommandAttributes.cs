namespace Gzzz.CommandInvoker;



[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
	public CommandAttribute(string path="", LoggingType loggingType= LoggingType.Detailed, bool useUpdate = false)
	{
		this.Path = string.IsNullOrEmpty(path) ? ""
			: path.StartsWith('/') ? path
			: "/"+ path;
		this.LoggingType = loggingType;
		this.UseUpdate = useUpdate;
	}

	public string Path { get; }
	public LoggingType LoggingType { get; }
	public bool UseUpdate { get; set; }
}


[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AnonymousCommandAttribute : CommandAttribute
{
	public AnonymousCommandAttribute(string path = "", LoggingType loggingType = LoggingType.Detailed) : base(path, loggingType,false)
	{
	}
}



[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class UpdateCommandAttribute : CommandAttribute
{
	public UpdateCommandAttribute(string path = "", LoggingType loggingType = LoggingType.Detailed) : base(path, loggingType, true)
	{
	}
}

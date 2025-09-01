using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.CommandInvoker;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ControllerAttribute : Attribute
{
    public string Path { get; }
    public ServiceLifetime ServiceLifetime { get; }
	public ControllerAttribute(string path= "", ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
	{
		this.Path = string.IsNullOrEmpty(path) ? ""
			: path.StartsWith('/') ? path
			: "/" + path;
		this.ServiceLifetime = serviceLifetime;
	}
}

using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.CommandInvoker;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ControllerAttribute : Attribute
{
    public string Prefix { get; }
    public ServiceLifetime ServiceLifetime { get; }
	public ControllerAttribute(string prefix= "", ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
	{
		Prefix = prefix;
		ServiceLifetime = serviceLifetime;
	}
	
}

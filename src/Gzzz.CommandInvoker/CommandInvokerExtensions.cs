using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;

namespace Gzzz.CommandInvoker;

public static class  CommandInvokerExtensions
{
	public static TServiceCollection AddCommandInvokers<TServiceCollection>(this TServiceCollection services, Assembly assembly) where TServiceCollection : IServiceCollection
		=> AddCommandInvokers(services, [assembly]);
	public static TServiceCollection AddCommandInvokers<TServiceCollection>(this TServiceCollection services, Assembly[] assemblies) where TServiceCollection : IServiceCollection
	{
		Dictionary<string, CommandInfo> commands = [];
		foreach (var assembly in assemblies)
		{
			foreach (var controllerType in assembly
				.GetTypes()
				.Where(type => type.IsClass && !type.IsInterface && !type.IsAbstract))
			{
				if (controllerType.GetCustomAttribute<ControllerAttribute>() == null)
					continue;

				var controllerAttribute = controllerType.GetCustomAttribute<ControllerAttribute>();
				services.Add(new ServiceDescriptor(controllerType, controllerType, controllerAttribute.ServiceLifetime));

				foreach (var methodInfo in controllerType.GetMethods())
				{
					var attr = methodInfo.GetCustomAttribute<CommandAttribute>();
					if (attr == default) continue;

					var path = controllerAttribute.Path + attr.Path;
					if (commands.TryGetValue(path, out var exists))
					{
						throw new Exception($"COMMAND PATH DUPLICATED : Controller:{controllerType}, Command:{methodInfo}");
					}
					commands.Add(path, new CommandInfo(methodInfo, attr));
				}
			}
		}
		services.AddSingleton<IReadOnlyDictionary<string, CommandInfo>>(new ReadOnlyDictionary<string, CommandInfo>(commands));
		return services;
	}
}

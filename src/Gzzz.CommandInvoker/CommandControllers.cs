using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

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

				var controllerAttr = controllerType.GetCustomAttribute<ControllerAttribute>();
				services.Add(new ServiceDescriptor(controllerType, controllerType, controllerAttr.ServiceLifetime));

				foreach (var methodInfo in controllerType.GetMethods())
				{
					var attr = methodInfo.GetCustomAttribute<CommandAttribute>();
					if (attr == default) continue;
					if (commands.TryGetValue(attr.Path, out var exists))
					{
						throw new Exception($"COMMAND PATH DUPLICATED : {attr.Path},{methodInfo.DeclaringType.FullName}/{methodInfo.Name}");
					}
					commands.Add(attr.Path, new CommandInfo(methodInfo, attr));
				}
			}
		}
		services.AddSingleton<IReadOnlyDictionary<string, CommandInfo>>(new ReadOnlyDictionary<string, CommandInfo>(commands));
		return services;
	}
}

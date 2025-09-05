using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Gzzz.CommandInvoker;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Project1.Controllers;
using Gzzz;
using Gzzz.Controllers;
using Gzzz.Authentication;
namespace Project1;

public static class Function
{
	private static async Task Main()
	{
		

		Assembly[] assemblies = [typeof(TestController).Assembly, typeof(SignController).Assembly];
		await new FunctionHandlerBuilder(CustomJsonContext.Default.Options)
			.UseCommandInvokers(assemblies)
			.UseAuthentication<AuthenticationService>()
			.ConfigureServices(services => services
				.AddSingleton<IContextSerializer, JsonContextSerializer>()
			).BuildFunctionHandler()
			.RunAsync();
	}
}

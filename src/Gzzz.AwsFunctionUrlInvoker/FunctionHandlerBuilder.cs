using Gzzz.AwsFunctionUrlInvoker.Models;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Gzzz.CommandInvoker;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker;

public delegate Task MiddlewareDelegateAsync(IServiceProvider services, FunctionUrlRequest request, ApiContext context);
public class FunctionHandlerBuilder
{
	readonly IServiceCollection _services = new ServiceCollection();

	public FunctionHandlerBuilder UseCommandInvokers(Assembly[] assemblies)
	{
		this._services.AddCommandInvokers(assemblies);
		return this;
	}
	public FunctionHandlerBuilder ConfigureServices(Action<IServiceCollection> configureServices)
	{
		configureServices(_services);
		return this;
	}

	public FunctionHandlerBuilder UseJsonSerializerOptions(JsonSerializerOptions options)
	{
		this._services.AddSingleton(options);
		return this;
	}

	public FunctionHandlerBuilder()
	{
		_services
			.AddSingleton<JsonLogger>()
			.AddSingleton<TimeService>()
			.AddScoped<ApiContext>();
	}

	public FunctionHandler BuildFunctionHandler()
	{
		var services = _services.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
		return new FunctionHandler(services);
	}

}

public interface IMiddleware
{
	public Task NextAsync(IServiceProvider services, FunctionUrlRequest request, ApiContext context);
}

public interface IAuthenticationMiddleware : IMiddleware
{
}

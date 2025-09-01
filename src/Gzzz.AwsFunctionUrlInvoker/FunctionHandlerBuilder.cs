using Gzzz.Authentication;
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
	readonly JsonSerializerOptions _options;

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
	public FunctionHandlerBuilder(JsonSerializerOptions options)
	{
		this._services
			.AddSingleton<IAccountScopedRepository, DefaultAccountScopedRepository>()
			.AddSingleton<JsonLogger>()
			.AddSingleton<TimeService>()
			.AddScoped<ApiContext>()
			.AddSingleton(options);
		_options = options;
	}

	public FunctionHandler BuildFunctionHandler()
	{
		var services = _services.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
		return new FunctionHandler(services);
	}

	public FunctionHandlerBuilder AddEnvironmentObject<T>(string name) where T : class
	{
		var json = EnvironmentX.GetValue(name);
		T obj = JsonSerializer.Deserialize<T>(json, _options) ?? throw new Exception($"환경변수 {name}의 객체 변환에 실패했습니다.");
		this._services.AddSingleton(obj);
		return this;
	}


	public FunctionHandlerBuilder UseAuthentication<TAuthenticationService>() where TAuthenticationService : AuthenticationService
	{
		_services
			.AddSingleton<AuthenticationService, TAuthenticationService>()
			.AddSingleton<TokenService>();

		return AddEnvironmentObject<AuthenticationConfig>("ZZ_AUTHENTICATION_CONFIG"); 
	}

}

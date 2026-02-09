using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Gzzz;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddEnvironmentObject<T>(this IServiceCollection services, string name, JsonSerializerOptions jsonSerializerOptions) where T : class
	{
		var json = EnvironmentX.GetValue(name);
		T obj = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions) ?? throw new Exception($"환경변수 {name}의 객체 변환에 실패했습니다.");
		return services.AddSingleton(obj);
	}

	public static IServiceProvider BuildWithValidation(this IServiceCollection services)
		=>services.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

}

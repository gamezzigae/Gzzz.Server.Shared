using Gzzz.Serialize;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gzzz.CommandInvoker;

public class CommandInfo
{
	public LoggingType LoggingType { get; set; }
	public bool IsAuthenticationRequired { get; }
	public bool IsParameterRequired { get; }
	public bool UseUpdate { get; }
	public Type ControllerType { get; }
	public Type RequestType { get; }
	public Type ResponseType { get; }
	public Func<object, object> GetResult { get; }
	public Invoke<Task> Invoke { get; }

	public Type[] ParameterTypes { get; }
	public int BodyParameterIndex { get; }

	public CommandInfo(MethodInfo methodInfo, CommandAttribute commandAttribute)
	{
		this.IsAuthenticationRequired = (commandAttribute is AnonymousCommandAttribute) == false;
		this.LoggingType= commandAttribute.LoggingType;
		this.UseUpdate = commandAttribute.UseUpdate;
		var parameters = methodInfo.GetParameters();

		this.ParameterTypes = parameters
			.Select(item => item.GetCustomAttribute<FromServiceAttribute>() == null ? null : item.ParameterType)
			.ToArray();

		if(ParameterTypes.Count(type=>type==null)> 1)
		{
			throw new Exception("암시적 body parameter는 1개까지만 가능합니다.");
		}
		BodyParameterIndex = Array.FindIndex(ParameterTypes, type=>type==null);

		var returnType = methodInfo.ReturnType;
        if (returnType.IsAssignableTo(typeof(Task)) == false)
        {
            throw new Exception("Task/Task<T> 비동기 함수만 지원합니다.");
        }
        if (returnType.IsGenericType) // Task<T>
        {
            var resultInfo = returnType.GetProperty("Result");
			ResponseType = resultInfo.PropertyType;
			GetResult = FastPropertyGetter.Create(resultInfo);
		}

		ControllerType = methodInfo.ReflectedType;
		IsParameterRequired = BodyParameterIndex >= 0;
		RequestType = IsParameterRequired ? parameters[BodyParameterIndex].ParameterType : null;

		Invoke = FastInvoker.Create<Task>(methodInfo);
	}

	public async Task<T> InvokeAsync<T>(IServiceProvider services, object parameter)
	{
		var obj = await InvokeAsync(services, parameter);
		return (T)obj;
	}


	public async Task<object> InvokeAsync(IServiceProvider services, object parameter)
	{
		var controller = services.GetRequiredService(this.ControllerType);

		Task task;
		if (ParameterTypes.Length > 0)
		{
			var parameters = new object[this.ParameterTypes.Length];
			for (int i = 0; i < this.ParameterTypes.Length; i++)
			{
				var parameterType = this.ParameterTypes[i];
				parameters[i] = parameterType == null ? parameter : services.GetRequiredService(parameterType);
			}
			task = this.Invoke(controller, parameters);
		}
		else
		{
			task = this.Invoke(controller, Array.Empty<object>());
		}

		await task;

		if (this.GetResult == null)
			return null;

		return this.GetResult(task);
	}
}

public class FromBodyAttribute : Attribute
{
}
public class FromServiceAttribute : Attribute
{
}

public enum ParameterBindingType
{
	None,
	FromBody,
	FromService,
}

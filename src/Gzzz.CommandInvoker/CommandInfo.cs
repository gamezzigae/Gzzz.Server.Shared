using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.CommandInvoker;

public class CommandInfo
{
	public LoggingType LoggingType { get; set; }
	public bool AuthenticationRequired { get; }
	public bool ParameterRequired { get; }
	public Type ControllerType { get; }
	public Type RequestType { get; }
	public Func<object, object> ResultGetter { get; }
	public Func<object, object[], object> Invoker { get; }
	

	public CommandInfo(MethodInfo methodInfo, CommandAttribute commandAttribute)
	{
		this.AuthenticationRequired = (commandAttribute is AnonymousCommandAttribute) == false;
		this.LoggingType= commandAttribute.LoggingType;
		var parameters = methodInfo.GetParameters();

        if(parameters.Length > 1)
        {   
            throw new Exception("Command는 최대 1개의 파라미터만 허용합니다.");
        }

        var returnType = methodInfo.ReturnType;
        if (returnType.IsAssignableTo(typeof(Task)) == false)
        {
            throw new Exception("Task/Task<T> 비동기 함수만 지원합니다.");
        }
        if (returnType.IsGenericType) // Task<T>
        {
            var resultInfo = returnType.GetProperty("Result");
			ResultGetter = PropertyAccessor.CreateGetter(resultInfo);
		}

		ControllerType = methodInfo.ReflectedType;
		ParameterRequired = parameters.Length > 0;
		RequestType = ParameterRequired ? parameters[0].ParameterType : null;

		Invoker = FastInvoker.Create<Task>(methodInfo);
	}

	public async Task<T> InvokeAsync<T>(IServiceProvider services, object parameter)
	{
		var obj = await InvokeAsync(services, parameter);
		return (T)obj;
	}

	public async Task<object> InvokeAsync(IServiceProvider services, object parameter)
	{
		var controller = services.GetRequiredService(this.ControllerType);
		var task = (Task)this.Invoker(controller, parameter == null ? Array.Empty<object>() : [parameter]);

		await task;

		if (this.ResultGetter == null)
			return null;

		return this.ResultGetter(task);
	}
}

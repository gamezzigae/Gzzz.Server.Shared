using Gzzz.Serialize;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.CommandInvoker;

public class CommandInfo
{
	public LoggingType LoggingType { get; set; }
	public bool IsAuthenticationRequired { get; }
	public bool IsParameterRequired { get; }
	public Type ControllerType { get; }
	public Type RequestType { get; }
	public Type ResponseType { get; }
	public Func<object, object> ResultGetter { get; }
	public Invoke<Task> Invoke { get; }
	

	public CommandInfo(MethodInfo methodInfo, CommandAttribute commandAttribute)
	{
		this.IsAuthenticationRequired = (commandAttribute is AnonymousCommandAttribute) == false;
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
			ResponseType = resultInfo.PropertyType;
			ResultGetter = PropertyAccessor.CreateGetter(resultInfo);
		}

		ControllerType = methodInfo.ReflectedType;
		IsParameterRequired = parameters.Length > 0;
		RequestType = IsParameterRequired ? parameters[0].ParameterType : null;

		Invoke = FastInvoker.Create<Task>(methodInfo);
	}

	public async Task<T> InvokeAsync<T>(IServiceProvider services, object parameter)
	{
		var obj = await InvokeAsync(services, parameter);
		return (T)obj;
	}


	object[] _parameters = new object[1];
	public async Task<object> InvokeAsync(IServiceProvider services, object parameter)
	{
		var controller = services.GetRequiredService(this.ControllerType);

		Task task;
		if(IsParameterRequired)
		{
			_parameters[0] = parameter; //aws lambda에서만 유효하기 때문에 멀티스레딩 환경에서는 사용하면 안됩니다.
			task = this.Invoke(controller, _parameters);
			_parameters[0] = null;
		}
		else
		{
			task = this.Invoke(controller, Array.Empty<object>());
		}

		await task;

		if (this.ResultGetter == null)
			return null;

		return this.ResultGetter(task);
	}
}

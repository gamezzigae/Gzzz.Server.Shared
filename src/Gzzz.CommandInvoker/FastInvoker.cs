using System.Linq.Expressions;
using System.Reflection;

namespace Gzzz.CommandInvoker;
public static class FastInvoker
{
    public static Func<object, object[], T> Create<T>(MethodInfo methodInfo)
    {
        var targetParam = Expression.Parameter(typeof(object), "target");
        var argsParam = Expression.Parameter(typeof(object[]), "args");

        var callArgs = new Expression[methodInfo.GetParameters().Length];
        var parameters = methodInfo.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            var index = Expression.Constant(i);
            var paramType = parameters[i].ParameterType;

            var accessor = Expression.ArrayIndex(argsParam, index);
            var cast = Expression.Convert(accessor, paramType);
            callArgs[i] = cast;
        }

        var instance = methodInfo.IsStatic ? null : Expression.Convert(targetParam, methodInfo.DeclaringType);
        var call = Expression.Call(instance, methodInfo, callArgs);

        Expression body = methodInfo.ReturnType == typeof(void)
            ? Expression.Block(call, Expression.Constant(null))
            : Expression.Convert(call, typeof(T));

        return Expression.Lambda<Func<object, object[], T>>(body, targetParam, argsParam).Compile();
    }
}

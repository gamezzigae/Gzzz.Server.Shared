using System.Linq.Expressions;
using System.Reflection;

namespace Gzzz.CommandInvoker;
public static class PropertyAccessor
{
	public static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
	{
		ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

		var instance = Expression.Parameter(typeof(object), "instance");

		// (TInstance)instance
		var instanceCast = !propertyInfo.DeclaringType!.IsValueType
			? Expression.TypeAs(instance, propertyInfo.DeclaringType)
			: Expression.Convert(instance, propertyInfo.DeclaringType);

		// instance.Property
		var propertyAccess = Expression.Property(instanceCast, propertyInfo);

		// boxing (object)
		var convert = Expression.Convert(propertyAccess, typeof(object));

		// 최종 람다: (object instance) => (object) ((TInstance)instance).Property
		return Expression.Lambda<Func<object, object>>(
			convert, instance
		).Compile();
	}
}

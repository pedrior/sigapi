using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Sigapi.Common.Scraping.Reflection;

public sealed class PropertyHelper
{
    private static readonly ConcurrentDictionary<(Type, string), Action<object, object>> Cache = new();

    public static void SetValue(PropertyInfo property, object instance, object value)
    {
        var key = (instance.GetType(), property.Name);
        var setter = Cache.GetOrAdd(key, _ => CompileSetterExpression(property));

        setter(instance, value);
    }

    private static Action<object, object> CompileSetterExpression(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        // Cast the instance and value to the correct types
        var instanceCast = Expression.Convert(instance, property.DeclaringType!);
        var valueCast = Expression.Convert(value, property.PropertyType);

        // Create the property access and assignment expressions
        var propertyAccess = Expression.Property(instanceCast, property);
        var assign = Expression.Assign(propertyAccess, valueCast);

        var lambda = Expression.Lambda<Action<object, object>>(assign, instance, value);
        
        return lambda.Compile();
    }
}
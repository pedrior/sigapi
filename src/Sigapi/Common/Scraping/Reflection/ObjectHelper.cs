using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Sigapi.Common.Scraping.Reflection;

public static class ObjectHelper
{
    private static readonly ConcurrentDictionary<(Type, Type), Func<object>> Cache = new();

    public static Func<T> BuildParameterlessConstructorDelegate<T>(Type type) where T : class
    {
        var key = (type, typeof(T));
        return (Func<T>)Cache.GetOrAdd(key, static tuple =>
        {
            var (targetType, returnType) = tuple;

            var ctor = targetType.GetConstructor(Type.EmptyTypes)
                       ?? throw new InvalidOperationException(
                           $"Type {targetType} must have a parameterless constructor.");

            var newExpr = Expression.New(ctor);
            var convert = Expression.Convert(newExpr, returnType);
            var lambda = Expression.Lambda<Func<T>>(convert);

            return lambda.Compile();
        });
    }
}
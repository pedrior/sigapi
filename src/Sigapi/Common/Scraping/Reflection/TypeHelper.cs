using System.Collections;

namespace Sigapi.Common.Scraping.Reflection;

public static class TypeHelper
{
    public static bool IsCollectionType(Type type) =>
        type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

    public static Type ResolveSingleCollectionItemType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType()!;
        }

        if (collectionType.IsGenericType)
        {
            return collectionType.GetGenericArguments()[0];
        }

        throw new ScrapingException($"Cannot determine item type for collection {collectionType.FullName}.");
    }

    public static T CreateCollectionFromItems<T>(IEnumerable<object> collection, Type itemType)
    {
        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var item in collection) list.Add(item);

        if (typeof(T).IsArray)
        {
            var array = Array.CreateInstance(itemType, list.Count);
            list.CopyTo(array, 0);
            return (T)(object)array;
        }

        return (T)list;
    }
}
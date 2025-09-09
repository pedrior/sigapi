using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using AngleSharp.Dom;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping;

public sealed class ScrapingService(
    IElementSelector elementSelector,
    IDataProcessingPipeline dataProcessingPipeline,
    ITypeConversionService typeConversionService) : IScrapingService
{
    private static readonly ConcurrentDictionary<Type, Func<object>> Factories = new();

    public T Scrape<T>(Page root) where T : new() => Scrape<T>(root.Document.DocumentElement);

    public T Scrape<T>(IDocument root) where T : new() => Scrape<T>(root.DocumentElement);

    public T Scrape<T>(IElement root) where T : new() => (T)ScrapeObject(typeof(T), root);

    public IEnumerable<T> ScrapeMany<T>(Page root) where T : new() => ScrapeMany<T>(root.Document.DocumentElement);

    public IEnumerable<T> ScrapeMany<T>(IDocument root) where T : new() => ScrapeMany<T>(root.DocumentElement);

    public IEnumerable<T> ScrapeMany<T>(IElement root) where T : new() => SelectManyObjects(typeof(T), root).Cast<T>();

    private object ScrapeObject(Type type, IElement parent)
    {
        var instance = CreateInstance(type);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            ProcessProperty(instance, property, parent);
        }

        return instance;
    }

    private IEnumerable<object> SelectManyObjects(Type type, IElement parent)
    {
        if (type.GetCustomAttribute<CollectionSelectorAttribute>() is not { } attribute)
        {
            throw new ScrapingException($"Type {type.FullName} must have a " +
                                        $"{nameof(CollectionSelectorAttribute)} to be scraped as a collection.");
        }

        var elements = elementSelector.SelectAll(parent, attribute.Selector);
        return elements.Select(element => ScrapeObject(type, element));
    }

    private void ProcessProperty(object instance, PropertyInfo property, IElement parent)
    {
        if (TryProcessComplexObject(instance, property, parent))
        {
            return;
        }

        if (TryProcessCollection(instance, property, parent))
        {
            return;
        }

        if (TryProcessDictionary(instance, property, parent))
        {
            return;
        }

        if (TryHandleElementExistence(instance, property, parent))
        {
            return;
        }

        _ = TryProcessPrimitiveProperty(instance, property, parent);
    }

    private bool TryProcessComplexObject(object instance, PropertyInfo property, IElement parent)
    {
        if (property.GetCustomAttribute<ComplexSelectorAttribute>() is not { } attribute)
        {
            return false;
        }

        var element = elementSelector.Select(parent, attribute.Selector);
        if (element is null)
        {
            ValidateRequiredElement(attribute.IsRequired, attribute.Selector);
            return true;
        }

        // Recurse to scrape the nested object.
        var nested = ScrapeObject(property.PropertyType, element);
        PropertyHelper.SetValue(property, instance, nested);

        return true;
    }

    private bool TryProcessCollection(object instance, PropertyInfo property, IElement parent)
    {
        if (property.GetCustomAttribute<CollectionSelectorAttribute>() is not { } attribute)
        {
            return false;
        }

        if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType) || !property.PropertyType.IsGenericType)
        {
            throw new ScrapingException(
                $"Property {property.Name} is marked as a collection but is not an IEnumerable.");
        }

        var elements = elementSelector.SelectAll(parent, attribute.Selector)
            .ToArray();

        if (elements.Length is 0)
        {
            return true; // No elements found, so we can skip this property.
        }

        var (list, itemType) = CreateTypedList(property.PropertyType);
        foreach (var element in elements)
        {
            if (IsComplexType(itemType))
            {
                // For complex types, we need to recurse to scrape the nested object.
                list.Add(ScrapeObject(itemType, element));
            }
            else
            {
                // For primitive types, we can extract the value directly.
                if (ExtractAndConvertValue(element, property, itemType, attribute.Attribute) is { } value)
                {
                    list.Add(value);
                }
            }
        }

        PropertyHelper.SetValue(property, instance, list);

        return true;
    }

    private bool TryProcessDictionary(object instance, PropertyInfo property, IElement parent)
    {
        if (property.GetCustomAttribute<AttributeSelectorAttribute>() is not { } attribute)
        {
            return false;
        }

        if (!typeof(IDictionary<string, string>).IsAssignableFrom(property.PropertyType) ||
            !property.PropertyType.IsGenericType)
        {
            throw new ScrapingException(
                $"Property {property.Name} is marked as a dictionary but is not an IDictionary<string, string>.");
        }

        var elements = elementSelector.SelectAll(parent, attribute.Selector)
            .ToArray();

        if (elements.Length is 0)
        {
            return true; // No elements found, so we can skip this property.
        }

        var dictionary = (IDictionary<string, string>)property.GetValue(instance)!;
        dictionary.Clear();

        foreach (var element in elements)
        {
            var key = element.GetAttribute(attribute.Key);
            if (string.IsNullOrEmpty(key))
            {
                throw new ScrapingException("Dictionary key is missing for element.");
            }

            var value = element.GetAttribute(attribute.Value) ?? string.Empty;

            dictionary.Add(key, value);
        }

        return true;
    }

    private bool TryHandleElementExistence(object instance, PropertyInfo property, IElement parent)
    {
        if (property.GetCustomAttribute<ExistsSelectorAttribute>() is not { } attribute)
        {
            return false;
        }

        if (property.PropertyType != typeof(bool))
        {
            throw new ScrapingException(
                $"Property {property.Name} is marked as existing but is not a boolean.");
        }

        var element = elementSelector.Select(parent, attribute.Selector);
        PropertyHelper.SetValue(property, instance, element is not null);

        return true;
    }

    private bool TryProcessPrimitiveProperty(object instance, PropertyInfo property, IElement parent)
    {
        if (property.GetCustomAttribute<ValueSelectorAttribute>() is not { } attribute)
        {
            return false;
        }

        var element = elementSelector.Select(parent, attribute.Selector);
        if (element is null)
        {
            ValidateRequiredElement(attribute.IsRequired, attribute.Selector);
            return true;
        }

        if (ExtractAndConvertValue(element, property, property.PropertyType, attribute.Attribute) is { } value)
        {
            PropertyHelper.SetValue(property, instance, value);
        }

        return true;
    }

    private object? ExtractAndConvertValue(IElement element,
        PropertyInfo property,
        Type targetType,
        string? attribute)
    {
        var raw = GetRawValue(element, attribute);
        var processed = dataProcessingPipeline.Process(raw,
            property.GetCustomAttributes<DataProcessorAttribute>());

        try
        {
            return typeConversionService.ConvertTo(processed, targetType);
        }
        catch (Exception ex)
        {
            throw new ScrapingException($"Failed to convert value for property {property.Name}", ex);
        }
    }

    private static object CreateInstance(Type type) =>
        Factories.GetOrAdd(type, ObjectHelper.BuildParameterlessConstructorDelegate<object>)();

    private static (IList list, Type itemType) CreateTypedList(Type propertyType)
    {
        var genericArguments = propertyType.GetGenericArguments();
        if (genericArguments.Length > 1)
        {
            throw new ScrapingException("Only single generic argument is supported for collections.");
        }

        var itemType = genericArguments[0];
        var listType = typeof(List<>)
            .MakeGenericType(itemType);

        var list = ObjectHelper.BuildParameterlessConstructorDelegate<IList>(listType)();
        return (list, itemType);
    }

    private static bool IsComplexType(Type type) => !type.IsPrimitive && type != typeof(string);

    private static void ValidateRequiredElement(bool isRequired, string selector)
    {
        if (isRequired)
        {
            throw new ElementNotFoundException($"Required element with selector '{selector}' not found.");
        }
    }

    private static string? GetRawValue(IElement element, string? attribute) =>
        string.IsNullOrEmpty(attribute) ? element.TextContent : element.GetAttribute(attribute);
}
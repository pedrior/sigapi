using System.Collections;
using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping.Strategies;

public sealed class CollectionPropertyStrategy : PropertyScrapingStrategy, IPropertyScrapingStrategy
{
    public CollectionPropertyStrategy(IDataProcessingPipeline dataProcessingPipeline,
        ITypeConversionService typeConversionService) : base(dataProcessingPipeline, typeConversionService)
    {
    }

    public bool Evaluate(PropertyInfo property) =>
        property.GetCustomAttribute<CollectionSelectorAttribute>() is not null;

    public void Execute(IScrapingContext context, object instance, PropertyInfo property, IElement parent)
    {
        var attribute = property.GetCustomAttribute<CollectionSelectorAttribute>()!;

        var elements = parent.QueryAll(attribute.Selector).ToArray();
        if (elements.Length is 0)
        {
            return;
        }

        var (list, itemType) = CreateTypedList(property.PropertyType);
        var isComplexItemType = !itemType.IsPrimitive && itemType != typeof(string);

        foreach (var element in elements)
        {
            if (isComplexItemType)
            {
                var nestedObject = context.ScrapeObject(itemType, element);
                list.Add(nestedObject);
            }
            else
            {
                if (ExtractAndConvertValue(element, property, itemType, attribute.Attribute) is { } value)
                {
                    list.Add(value);
                }
            }
        }

        PropertyHelper.SetValue(property, instance, list);
    }

    private static (IList list, Type itemType) CreateTypedList(Type propertyType)
    {
        var itemType = propertyType.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = (IList)Activator.CreateInstance(listType)!;

        return (list, itemType);
    }
}
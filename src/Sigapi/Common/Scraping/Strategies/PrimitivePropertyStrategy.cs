using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping.Strategies;

public sealed class PrimitivePropertyStrategy : PropertyScrapingStrategy, IPropertyScrapingStrategy
{
    public PrimitivePropertyStrategy(IDataProcessingPipeline dataProcessingPipeline,
        ITypeConversionService typeConversionService) : base(dataProcessingPipeline, typeConversionService)
    {
    }

    public bool Evaluate(PropertyInfo property) =>
        property.GetCustomAttribute<ValueSelectorAttribute>() is not null;

    public void Execute(IScrapingContext context, object instance, PropertyInfo property, IElement parent)
    {
        var attribute = property.GetCustomAttribute<ValueSelectorAttribute>()!;
        var element = parent.Query(attribute.Selector);

        if (element is null)
        {
            ValidateRequiredElement(attribute.IsRequired, attribute.Selector, property.Name);
            return;
        }

        if (ExtractAndConvertValue(element, property, property.PropertyType, attribute.Attribute) is { } value)
        {
            PropertyHelper.SetValue(property, instance, value);
        }
    }
}
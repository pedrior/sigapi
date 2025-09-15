using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping.Strategies;

public sealed class ComplexObjectPropertyStrategy : PropertyScrapingStrategy, IPropertyScrapingStrategy
{
    public ComplexObjectPropertyStrategy(IDataProcessingPipeline dataProcessingPipeline,
        ITypeConversionService typeConversionService) : base(dataProcessingPipeline, typeConversionService)
    {
    }

    public bool Evaluate(PropertyInfo property) => property
        .GetCustomAttribute<ComplexSelectorAttribute>() is not null;

    public void Execute(IScrapingContext context, object instance, PropertyInfo property, IElement parent)
    {
        var attribute = property.GetCustomAttribute<ComplexSelectorAttribute>()!;
        var element = parent.Query(attribute.Selector);

        if (element is null)
        {
            ValidateRequiredElement(attribute.IsRequired, attribute.Selector, property.Name);
            return;
        }

        var nestedObject = context.ScrapeObject(property.PropertyType, element);
        
        PropertyHelper.SetValue(property, instance, nestedObject);
    }
}
using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping.Strategies;

public sealed class ElementExistsPropertyStrategy : PropertyScrapingStrategy, IPropertyScrapingStrategy
{
    public ElementExistsPropertyStrategy(IDataProcessingPipeline dataProcessingPipeline,
        ITypeConversionService typeConversionService) : base(dataProcessingPipeline, typeConversionService)
    {
    }

    public bool Evaluate(PropertyInfo property) =>
        property.GetCustomAttribute<ExistsSelectorAttribute>() is not null;

    public void Execute(IScrapingContext context, object instance, PropertyInfo property, IElement parent)
    {
        var attribute = property.GetCustomAttribute<ExistsSelectorAttribute>()!;

        if (property.PropertyType != typeof(bool))
        {
            throw new ScrapingException(
                $"Property {property.Name} is marked with {nameof(ExistsSelectorAttribute)} but is not a boolean.");
        }

        var element = parent.Query(attribute.Selector);
        PropertyHelper.SetValue(property, instance, element is not null);
    }
}
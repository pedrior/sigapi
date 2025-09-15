using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping.Strategies;

public class DictionaryPropertyStrategy : PropertyScrapingStrategy, IPropertyScrapingStrategy
{
    public DictionaryPropertyStrategy(IDataProcessingPipeline dataProcessingPipeline,
        ITypeConversionService typeConversionService) : base(dataProcessingPipeline, typeConversionService)
    {
    }

    public bool Evaluate(PropertyInfo property) =>
        property.GetCustomAttribute<AttributeSelectorAttribute>() is not null;

    public void Execute(IScrapingContext context, object instance, PropertyInfo property, IElement parent)
    {
        var attribute = property.GetCustomAttribute<AttributeSelectorAttribute>()!;

        if (!typeof(IDictionary<string, string>).IsAssignableFrom(property.PropertyType))
        {
            throw new ScrapingException(
                $"Property {property.Name} is marked with {nameof(AttributeSelectorAttribute)} " +
                $"but is not an IDictionary<string, string>.");
        }

        var elements = parent.QueryAll(attribute.Selector).ToArray();
        if (elements.Length is 0)
        {
            return; // No elements found, nothing to do.
        }

        var dictionary = (IDictionary<string, string>?)property.GetValue(instance);
        if (dictionary is null)
        {
            // If the dictionary is null, create a new one. Assumes the property is settable.
            dictionary = new Dictionary<string, string>();
            PropertyHelper.SetValue(property, instance, dictionary);
        }
        else
        {
            dictionary.Clear();
        }

        foreach (var element in elements)
        {
            var key = element.GetAttribute(attribute.Key);
            if (string.IsNullOrEmpty(key))
            {
                // Skip elements where the key is missing or empty.
                continue;
            }

            // Use indexer to add or update.
            dictionary[key] = element.GetAttribute(attribute.Value) ?? string.Empty;
        }
    }
}
using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Common.Scraping.Strategies;

public abstract class PropertyScrapingStrategy
{
    private readonly IDataProcessingPipeline dataProcessingPipeline;
    private readonly ITypeConversionService typeConversionService;

    protected PropertyScrapingStrategy(IDataProcessingPipeline dataProcessingPipeline, 
        ITypeConversionService typeConversionService)
    {
        this.dataProcessingPipeline = dataProcessingPipeline;
        this.typeConversionService = typeConversionService;
    }
    
    protected object? ExtractAndConvertValue(IElement element,
        PropertyInfo property,
        Type targetType,
        string? attributeName)
    {
        var rawValue = string.IsNullOrEmpty(attributeName)
            ? element.GetText()
            : element.GetAttribute(attributeName);

        var processedValue = dataProcessingPipeline.Process(rawValue,
            property.GetCustomAttributes<DataProcessorAttribute>());

        try
        {
            return typeConversionService.ConvertTo(processedValue, targetType);
        }
        catch (Exception ex)
        {
            throw new ScrapingException($"Failed to convert value for property {property.Name}", ex);
        }
    }

    protected static void ValidateRequiredElement(bool isRequired, string selector, string property)
    {
        if (isRequired)
        {
            throw new ElementNotFoundException(selector, property);
        }
    }
}
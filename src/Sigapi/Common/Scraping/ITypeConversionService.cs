namespace Sigapi.Common.Scraping;

public interface ITypeConversionService
{
    object? ConvertTo(object? input, Type targetType);
}
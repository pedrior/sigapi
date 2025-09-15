namespace Sigapi.Common.Scraping.Reflection;

public interface ITypeMetadataProvider
{
    TypeMetadata GetMetadata(Type type);
}
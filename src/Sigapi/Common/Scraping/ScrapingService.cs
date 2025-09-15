using System.Reflection;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Reflection;

namespace Sigapi.Common.Scraping;

public sealed class ScrapingService : IScrapingService, IScrapingContext
{
    private readonly ITypeMetadataProvider typeMetadataProvider;

    public ScrapingService(ITypeMetadataProvider typeMetadataProvider)
    {
        this.typeMetadataProvider = typeMetadataProvider;
    }
    
    public T Scrape<T>(IElement element) where T : class
    {
        var targetType = typeof(T);

        if (!TypeHelper.IsCollectionType(targetType))
        {
            return (T)ScrapeObject(targetType, element);
        }

        var itemType = TypeHelper.ResolveSingleCollectionItemType(targetType);
        var collection = ScrapeObjectCollection(itemType, element);

        return TypeHelper.CreateCollectionFromItems<T>(collection, itemType);
    }

    public object ScrapeObject(Type type, IElement parent)
    {
        var metadata = typeMetadataProvider.GetMetadata(type);
        var instance = metadata.Factory();

        foreach (var mapping in metadata.PropertyMappings)
        {
            mapping.Strategy.Execute(this, instance, mapping.Property, parent);
        }

        return instance;
    }

    public IEnumerable<object> ScrapeObjectCollection(Type itemType, IElement parent)
    {
        var attribute = itemType.GetCustomAttribute<CollectionSelectorAttribute>();
        if (attribute is null)
        {
            throw new ScrapingException($"Type {itemType.FullName} must have a " +
                                        $"{nameof(CollectionSelectorAttribute)} to be scraped as a collection.");
        }

        var elements = parent.QueryAll(attribute.Selector);
        return elements.Select(element => ScrapeObject(itemType, element));
    }
}
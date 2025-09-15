using System.Collections.Concurrent;
using Sigapi.Common.Scraping.Strategies;

namespace Sigapi.Common.Scraping.Reflection;

public sealed class TypeMetadataProvider : ITypeMetadataProvider
{
    private readonly IEnumerable<IPropertyScrapingStrategy> strategies;
    private readonly ConcurrentDictionary<Type, TypeMetadata> cache = new();

    public TypeMetadataProvider(IEnumerable<IPropertyScrapingStrategy> strategies)
    {
        this.strategies = strategies;
    }

    public TypeMetadata GetMetadata(Type type) => cache.GetOrAdd(type, CreateMetadata);

    private TypeMetadata CreateMetadata(Type type)
    {
        var mappings = new List<PropertyStrategyMapping>();

        foreach (var property in type.GetProperties())
        {
            if (strategies.FirstOrDefault(s => s.Evaluate(property)) is { } strategy)
            {
                mappings.Add(new PropertyStrategyMapping(property, strategy));
            }
        }

        return new TypeMetadata(ObjectHelper.BuildParameterlessConstructorDelegate<object>(type), mappings);
    }
}
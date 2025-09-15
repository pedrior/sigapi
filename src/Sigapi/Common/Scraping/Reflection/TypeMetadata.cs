using Sigapi.Common.Scraping.Strategies;

namespace Sigapi.Common.Scraping.Reflection;

public sealed class TypeMetadata
{
    public TypeMetadata(Func<object> factory, IReadOnlyList<PropertyStrategyMapping> propertyMappings)
    {
        Factory = factory;
        PropertyMappings = propertyMappings;
    }
    
    public Func<object> Factory { get; }
    
    public IReadOnlyList<PropertyStrategyMapping> PropertyMappings { get; }
}
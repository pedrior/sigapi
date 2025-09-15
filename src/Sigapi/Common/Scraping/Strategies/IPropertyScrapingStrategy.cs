using System.Reflection;
using Sigapi.Common.Scraping.Document;

namespace Sigapi.Common.Scraping.Strategies;

public interface IPropertyScrapingStrategy
{
    bool Evaluate(PropertyInfo property);
    
    void Execute(IScrapingContext context, object instance, PropertyInfo property, IElement parent);
}
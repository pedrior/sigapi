using System.Reflection;

namespace Sigapi.Common.Scraping.Strategies;

public sealed record PropertyStrategyMapping(PropertyInfo Property, IPropertyScrapingStrategy Strategy);
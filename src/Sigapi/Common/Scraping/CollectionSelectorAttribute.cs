namespace Sigapi.Common.Scraping;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class CollectionSelectorAttribute(string selector) : SelectorAttribute(selector);
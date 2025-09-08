namespace Sigapi.Common.Scraping.Processing;

public sealed class AbsoluteUrlAttribute(int order = 0) : DataProcessorAttribute(AbsoluteUrlProcessor.Name, order);
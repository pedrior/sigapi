namespace Sigapi.Common.Scraping.Processing;

public sealed class NormalizeWhitespaceAttribute(int order = 0)
    : DataProcessorAttribute(NormalizeWhitespaceProcessor.Name, order);
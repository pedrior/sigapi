namespace Sigapi.Common.Scraping.Processing;

public sealed class NullIfEmptyAttribute(int order = 0) : DataProcessorAttribute(NullIfEmptyProcessor.Name, order);
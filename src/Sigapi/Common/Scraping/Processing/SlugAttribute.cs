namespace Sigapi.Common.Scraping.Processing;

public sealed class SlugAttribute(int order = 0) : DataProcessorAttribute(SlugProcessor.Name, order);
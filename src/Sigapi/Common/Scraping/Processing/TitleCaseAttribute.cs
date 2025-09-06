namespace Sigapi.Common.Scraping.Processing;

public sealed class TitleCaseAttribute(int order = 0) : DataProcessorAttribute(TitleCaseProcessor.Name, order);
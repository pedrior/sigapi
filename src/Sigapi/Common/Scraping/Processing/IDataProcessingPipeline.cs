namespace Sigapi.Common.Scraping.Processing;

public interface IDataProcessingPipeline
{
    object? Process(object? value, IEnumerable<DataProcessorAttribute> attributes);
}
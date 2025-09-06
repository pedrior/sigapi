namespace Sigapi.Common.Scraping.Processing;

public interface IDataProcessor
{
    string UniqueName { get; }

    object? Process(object? input, IDictionary<string, string>? parameters);
}
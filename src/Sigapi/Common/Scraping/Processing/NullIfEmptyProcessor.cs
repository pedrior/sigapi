namespace Sigapi.Common.Scraping.Processing;

public sealed class NullIfEmptyProcessor : IDataProcessor
{
    public const string Name = "null-if-empty";
    
    public string UniqueName => Name;
    
    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string str)
        {
            return input;
        }

        return string.IsNullOrWhiteSpace(str) ? null : str;
    }
}
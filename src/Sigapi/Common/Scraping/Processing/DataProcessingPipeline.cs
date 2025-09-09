namespace Sigapi.Common.Scraping.Processing;

public sealed class DataProcessingPipeline(
    IEnumerable<IDataProcessor> processors,
    ILogger<DataProcessingPipeline> logger) : IDataProcessingPipeline
{
    // Create a lookup for efficient retrieval by name.
    private readonly ILookup<string, IDataProcessor> processors = processors
        .ToLookup(x => x.UniqueName, StringComparer.OrdinalIgnoreCase);

    public object? Process(object? value, IEnumerable<DataProcessorAttribute> attributes)
    {
        var current = value;

        // Process attributes in order of their specified order.
        foreach (var attribute in attributes.OrderBy(x => x.Order))
        {
            var processor = processors[attribute.Name].FirstOrDefault();
            if (processor is null)
            {
                logger.LogWarning("No data processor found for {Name}, skipping", attribute.Name);
                continue;
            }

            logger.LogDebug("Processing {Name} with parameters: {Parameters}", attribute.Name, attribute.Parameters);

            var parameters = ParseParameters(attribute.Parameters);

            try
            {
                current = processor.Process(current, parameters);
            }
            catch (Exception ex)
            {
                throw new DataProcessorException($"Failed to process {attribute.Name}", ex);
            }
        }

        return current;
    }

    private static Dictionary<string, string>? ParseParameters(string? parameters)
    {
        if (string.IsNullOrEmpty(parameters))
        {
            return null;
        }

        const char pairSeparator = ';';
        const char keyValueSeparator = '=';

        try
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var pairs = parameters.Split(pairSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var index = pair.IndexOf(keyValueSeparator);
                if (index > 0)
                {
                    var key = pair[..index].Trim();
                    var value = pair[(index + 1)..].Trim();

                    if (!result.TryAdd(key, value))
                    {
                        throw new FormatException($"Duplicate key '{key}' found.");
                    }
                }
                else
                {
                    throw new FormatException($"Invalid parameter format: '{pair}'. Expected format is 'key=value'.");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Failed to parse data processor parameters: {parameters}", ex);
        }
    }
}
namespace Sigapi.Common.Scraping.Processing;

public sealed class DataProcessorException(string message, Exception? inner = null) : Exception(message, inner);
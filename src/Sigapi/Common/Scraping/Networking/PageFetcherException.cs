namespace Sigapi.Common.Scraping.Networking;

public sealed class PageFetcherException(string message, Exception? inner = null) : Exception(message, inner);
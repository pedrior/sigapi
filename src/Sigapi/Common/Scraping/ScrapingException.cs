namespace Sigapi.Common.Scraping;

public class ScrapingException(string message, Exception? inner = null) : Exception(message, inner);
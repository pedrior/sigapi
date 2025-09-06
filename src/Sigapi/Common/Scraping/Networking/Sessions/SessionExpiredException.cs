namespace Sigapi.Common.Scraping.Networking.Sessions;

public class SessionExpiredException(string message, Exception? inner = null) : Exception(message, inner);
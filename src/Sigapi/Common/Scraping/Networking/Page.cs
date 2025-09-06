using AngleSharp.Dom;
using ISession = Sigapi.Common.Scraping.Networking.Sessions.ISession;

namespace Sigapi.Common.Scraping.Networking;

public sealed class Page(string url, IDocument document, ISession? session)
{
    public string Url => url;
    
    public IDocument Document => document;
    
    public ISession? Session => session;
}
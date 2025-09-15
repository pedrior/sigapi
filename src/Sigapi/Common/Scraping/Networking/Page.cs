using Sigapi.Common.Scraping.Document;
using ISession = Sigapi.Common.Scraping.Networking.Sessions.ISession;

namespace Sigapi.Common.Scraping.Networking;

public sealed class Page(string url, IElement element, ISession? session) : IElement
{
    public string Url => url;
    
    public IElement Element => element;
    
    public ISession? Session => session;
    
    public string? GetText() => element.GetText();

    string? IElement.GetAttribute(string name) => element.GetAttribute(name);

    IElement? IElement.Query(string selector) => element.Query(selector);

    IEnumerable<IElement> IElement.QueryAll(string selector) => element.QueryAll(selector);
}
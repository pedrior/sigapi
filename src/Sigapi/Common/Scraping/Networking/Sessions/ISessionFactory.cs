namespace Sigapi.Common.Scraping.Networking.Sessions;

public interface ISessionFactory
{
    Task<ISession?> Create(SessionPolicy policy);
}
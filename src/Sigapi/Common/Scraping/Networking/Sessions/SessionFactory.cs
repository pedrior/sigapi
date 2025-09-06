namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class SessionFactory(IServiceProvider serviceProvider) : ISessionFactory
{
    public async Task<ISession?> Create(SessionPolicy policy)
    {
        if (policy is SessionPolicy.None)
        {
            return null;
        }

        return await serviceProvider.GetRequiredKeyedService<ISessionProvider>(policy)
            .GetSessionAsync();
    }
}
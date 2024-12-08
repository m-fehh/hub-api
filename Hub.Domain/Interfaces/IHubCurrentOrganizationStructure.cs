using Hub.Domain.Entity;

namespace Hub.Domain.Interfaces
{
    public interface IHubCurrentOrganizationStructure
    {
        string GetCurrentDomain(string structId = null);
        OrganizationalStructure GetCurrentRoot();
        long? GetCurrentRootId();
        string Get();
        void Set(string id);
        void SetCookieRequest(string id);
        List<long> UpdateUser(long userid);
    }
}

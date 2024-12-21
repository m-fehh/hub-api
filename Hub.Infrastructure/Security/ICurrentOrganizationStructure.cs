using Hub.Infrastructure.Database.Models;

namespace Hub.Infrastructure.Security
{
    /// <summary>
    /// Interface usada pela plataforma ELOS (eseus módulos) para identificar qual o atual nível organizacional selecionado pelo usuário.
    /// Para sistemas satélites (como e-commerce ou app do consumidor), não se aplica.
    /// </summary>
    public interface ICurrentOrganizationStructure
    {
        OrganizationalStructureVM GetCurrentDomain(long? structId = null);
        OrganizationalStructureVM GetCurrentRoot();
        OrganizationalStructureVM GetCurrent();
        OrganizationalStructureVM Set(long id);

        void Set(OrganizationalStructureVM org);
        void SetByCookieData(string cookieData);

        TimeZoneInfo GetCurrentTimezone();
    }
}

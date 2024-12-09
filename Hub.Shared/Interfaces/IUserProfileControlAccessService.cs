using Hub.Shared.Models.VMs;

namespace Hub.Shared.Interfaces
{
    /// <summary>
    /// Interface resposável por validar o controle de acesso
    /// verificando se permite ou não o múltiplo acesso.
    /// </summary>
    public interface IUserProfileControlAccessService
    {
        void Validate(ProfileControlAccessVM parameter);
        void Save(ProfileControlAccessVM profileControlAccess, bool allowMultipleAccess);
    }
}

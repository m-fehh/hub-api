using Hub.Shared.Enums.Infrastructure;

namespace Hub.Application.Services
{
    public class LogService
    {
        public void LogMessage(string objectName, long objectId, ELogAction action, long ownerOrgStructId, string code = "", string status = "", string message = "")
        {
        }
    }
}

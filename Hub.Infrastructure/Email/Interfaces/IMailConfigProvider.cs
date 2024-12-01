using Hub.Infrastructure.Email.Models;

namespace Hub.Infrastructure.Email.Interfaces
{
    public interface IMailConfigProvider
    {
        MailConfig GetConfig(string key);
    }
}

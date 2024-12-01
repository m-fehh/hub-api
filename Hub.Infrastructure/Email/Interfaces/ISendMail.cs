using Hub.Infrastructure.Email.Models;
using System.Net;

namespace Hub.Infrastructure.Email.Interfaces
{
    public interface ISendMail
    {
        Task<HttpStatusCode?> Send(SendMailVM model);
    }
}

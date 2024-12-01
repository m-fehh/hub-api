using Hub.API.Configuration.Context;
using Hub.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Hub.API.Controllers
{
    /// <summary>
    /// Classe base para todos os controllers que utilizam o contexto do locatário.
    /// </summary>
    [ApiController]
    [Route("{tenantName}/api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected readonly string TenantName;

        protected BaseController()
        {
            TenantName = Engine.Resolve<ITenantContext>().TenantName;
        }
    }
}

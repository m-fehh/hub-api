using Hub.Infrastructure;
using Hub.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace Hub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {

        [HttpPost("migrate-tenants")]
        public IActionResult MigrateTenants()
        {
            //_tenantService.MigrateTenants();

            Engine.Resolve<TenantService>().MigrateTenants();

            return Ok("Migrações aplicadas com sucesso.");
        }
    }
}

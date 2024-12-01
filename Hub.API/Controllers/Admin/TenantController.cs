using Hub.API.Configuration.Filters;
using Hub.Application.Services.Admin;
using Hub.Infrastructure;
using Hub.Shared.Models.VMs;
using Microsoft.AspNetCore.Mvc;

namespace Hub.API.Controllers.Admin
{
    [ApiController]
    [SystemAuthorization]
    [Route("api/[controller]")]
    public class TenantController : BaseController
    {
        [HttpPost]
        public IActionResult InsertAsync([FromBody] TenantVM model)
        {
            Engine.Resolve<TenantService>().Insert(model);

            return new OkObjectResult(new
            {
                error = false
            });
        }
    }
}

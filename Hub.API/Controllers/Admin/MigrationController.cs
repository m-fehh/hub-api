using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement.Migrations;
using Hub.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Hub.API.Controllers.Admin
{
    public class MigrationController : BaseController
    {
        public MigrationController() : base() { }

        [HttpPost("run")]
        public IActionResult RunMigration()
        {
            using (Engine.BeginLifetimeScope(TenantName, true))
            {
                try
                {
                    Engine.Resolve<IMigrationRunner>().MigrateToLatest();

                    return new OkObjectResult(new
                    {
                        error = false
                    });
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new
                    {
                        error = true,
                        message = ex.CreateExceptionString()
                    });
                }
            }
        }
    }
}

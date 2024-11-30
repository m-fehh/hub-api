using Hub.API.Configuration;
using Hub.Infrastructure;
using Hub.Infrastructure.Database.NhManagement;
using Hub.Shared.DataConfiguration.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Hub.API.Controllers
{
    [ApiController]
    [Route("{tenantName}/api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("check-tenant")]
        public IActionResult CheckTenant()
        {
            var tenantName = Engine.Resolve<ITenantContext>().TenantName;
            using (Engine.BeginLifetimeScope(tenantName))
            { 
                var all = Engine.Resolve<IRepository<Tenants>>().Table.ToList();
            }

            // Retorna um HTTP 200 OK vazio
            return Ok();
        }
    }
}

using Hub.Application.Services;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure;
using Hub.Shared.Models.VMs;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Hub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Faz autenticação através de login e senha
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login([FromBody] AuthVM request)
        {
            try
            {
                request.Provider = EAuthProvider.Api;

                var userService = Engine.Resolve<UserService>();

                userService.Authenticate(request);

                if (request.Token == null)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized);
                }

                var userData = userService.AuthenticateToken(request.Token);

                return StatusCode((int)HttpStatusCode.OK, new { error = false, request.Token, Data = userData });
            }
            catch (BusinessException bex)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new { error = true, message = bex.Message });
            }
            catch (Exception ex)
            {
                //var log = Engine.Resolve<LogService>().LogError(ex, "API-Auth-Login");

                var msg = string.Format(Engine.Get("AnErrorWasOcurredReportNumber"), 1);

                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, message = msg });
            }
        }
    }
}

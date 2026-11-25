
using Microsoft.AspNetCore.Mvc;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                estado = "OK",
                mensaje = "API del Portal de Seguimiento a Graduados funcionando"
            });
        }
    }
}

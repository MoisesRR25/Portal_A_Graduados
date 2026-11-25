
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("health/database")]
    public class DatabaseHealthController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public DatabaseHealthController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            CancellationToken cancellationToken)
        {
            bool conectado = await _context.Database
                .CanConnectAsync(cancellationToken);

            if (!conectado)
            {
                return StatusCode(503, new
                {
                    estado = "ERROR",
                    mensaje = "No se pudo conectar a PostgreSQL"
                });
            }

            return Ok(new
            {
                estado = "OK",
                mensaje = "Conexion con PostgreSQL exitosa"
            });
        }
    }
}

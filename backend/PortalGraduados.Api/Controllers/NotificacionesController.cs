
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;

namespace PortalGraduados.Api.Controllers
{
    [Authorize(Roles = "Administrador de la Escuela,Graduado,Empresa")]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public NotificacionesController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // IDENTIFICAR USUARIO AUTENTICADO
        // ==========================================

        private int? ObtenerUsuarioActual()
        {
            return int.TryParse(
                User.FindFirst("sub")?.Value,
                out int idUsuario)
                ? idUsuario
                : null;
        }

        // ==========================================
        // LISTAR NOTIFICACIONES PROPIAS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] bool soloNoLeidas = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            if (pagina < 1 ||
                tamano < 1 ||
                tamano > 100)
            {
                return BadRequest(new
                {
                    mensaje = "Página o tamaño inválidos."
                });
            }

            var consulta = _context.Notificaciones
                .AsNoTracking()
                .Where(n =>
                    n.IdUsuario == idUsuario.Value);

            if (soloNoLeidas)
            {
                consulta = consulta.Where(n =>
                    !n.Leida);
            }

            var total = await consulta.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var lista = await consulta
                .OrderByDescending(n => n.Fecha)
                .ThenByDescending(n => n.IdNotificacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(n => new
                {
                    n.IdNotificacion,
                    n.Tipo,
                    n.Mensaje,
                    n.Fecha,
                    n.Leida
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ==========================================
        // CONTAR NOTIFICACIONES NO LEÍDAS
        // ==========================================

        [HttpGet("no-leidas")]
        public async Task<IActionResult> ContarNoLeidas()
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var total = await _context.Notificaciones
                .CountAsync(n =>
                    n.IdUsuario == idUsuario.Value &&
                    !n.Leida);

            return Ok(new
            {
                total
            });
        }

        // ==========================================
        // MARCAR NOTIFICACIÓN COMO LEÍDA
        // ==========================================

        [HttpPut("{id:int}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var notificacion =
                await _context.Notificaciones
                    .FirstOrDefaultAsync(n =>
                        n.IdNotificacion == id &&
                        n.IdUsuario == idUsuario.Value);

            if (notificacion is null)
            {
                return NotFound(new
                {
                    mensaje = "Notificación no disponible."
                });
            }

            notificacion.Leida = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                notificacion.IdNotificacion,
                notificacion.Leida
            });
        }

        // ==========================================
        // MARCAR TODAS COMO LEÍDAS
        // ==========================================

        [HttpPut("leer-todas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var actualizadas =
                await _context.Notificaciones
                    .Where(n =>
                        n.IdUsuario == idUsuario.Value &&
                        !n.Leida)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(
                            n => n.Leida,
                            true));

            return Ok(new
            {
                mensaje =
                    "Notificaciones marcadas como leídas.",

                actualizadas
            });
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public ReportesController(
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
        // RF-18: REPORTAR OFERTA
        // ==========================================

        [Authorize(Roles = "Graduado")]
        [HttpPost]
        public async Task<ActionResult<ReporteOfertaDto>>
            Crear([FromBody] ReporteOfertaCrearDto dto)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var motivo = dto.Motivo.Trim();

            if (motivo.Length < 10)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Describe el motivo con al menos 10 caracteres."
                });
            }

            var oferta = await _context.Ofertas
                .Include(o => o.Empresa)
                .ThenInclude(e => e.Usuario)
                .FirstOrDefaultAsync(o =>
                    o.IdOferta == dto.IdOferta);

            if (oferta is null)
                return NotFound();

            var ahora = DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);

            // Evitar reportes repetidos del mismo usuario.

            if (await _context.ReportesOferta.AnyAsync(r =>
                r.IdUsuario == idUsuario.Value &&
                r.IdOferta == dto.IdOferta))
            {
                return Conflict(new
                {
                    mensaje = "Ya reportaste esta oferta."
                });
            }

            // Solo se reportan ofertas visibles y vigentes.

            if (oferta.Estado != "Publicada" ||
                oferta.FechaCierre <= ahora ||
                !oferta.Empresa.EstadoAprobacion ||
                !oferta.Empresa.Usuario.EstadoCuenta)
            {
                return Conflict(new
                {
                    mensaje = "La oferta no está disponible."
                });
            }

            var reporte = new ReporteOferta
            {
                IdUsuario = idUsuario.Value,
                IdOferta = oferta.IdOferta,
                Motivo = motivo,
                FechaReporte = ahora
            };

            _context.ReportesOferta.Add(reporte);

            // RF-21: enviar la oferta a revisión.

            oferta.Estado = "En revision";

            // Notificar a los administradores activos.

            var administradores =
                await _context.Usuarios
                    .Where(u =>
                        u.EstadoCuenta &&
                        u.Rol.NombreRol ==
                            "Administrador de la Escuela")
                    .Select(u => u.IdUsuario)
                    .ToListAsync();

            foreach (var idAdministrador in administradores)
            {
                _context.Notificaciones.Add(
                    new Notificacion
                    {
                        IdUsuario = idAdministrador,
                        Tipo = "Reporte",
                        Mensaje =
                            $"La oferta \"{oferta.Cargo}\" " +
                            "fue reportada y requiere revisión.",
                        Fecha = ahora,
                        Leida = false
                    });
            }

            // Notificar a la empresa propietaria.

            _context.Notificaciones.Add(
                new Notificacion
                {
                    IdUsuario = oferta.Empresa.IdUsuario,
                    Tipo = "Oferta",
                    Mensaje =
                        $"Tu oferta \"{oferta.Cargo}\" " +
                        "fue enviada a revisión administrativa.",
                    Fecha = ahora,
                    Leida = false
                });

            // Reporte, oferta y notificaciones se
            // guardan en una misma transacción.

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new ReporteOfertaDto
                {
                    IdReporte = reporte.IdReporte,
                    IdUsuario = reporte.IdUsuario,
                    IdOferta = reporte.IdOferta,
                    Cargo = oferta.Cargo,
                    Empresa = oferta.Empresa.NombreComercial,
                    Motivo = reporte.Motivo,
                    FechaReporte = reporte.FechaReporte,
                    EstadoOferta = oferta.Estado
                });
        }

        // ==========================================
        // LISTAR REPORTES
        // Exclusivo del administrador.
        // ==========================================

        [Authorize(Roles = "Administrador de la Escuela")]
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] int? idOferta,
            [FromQuery] bool soloPendientes = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            if (pagina < 1 ||
                tamano < 1 ||
                tamano > 100)
            {
                return BadRequest(new
                {
                    mensaje = "Página o tamaño inválidos."
                });
            }

            var consulta = _context.ReportesOferta
                .AsNoTracking();

            if (idOferta.HasValue)
            {
                consulta = consulta.Where(r =>
                    r.IdOferta == idOferta.Value);
            }

            if (soloPendientes)
            {
                consulta = consulta.Where(r =>
                    r.Oferta.Estado == "En revision");
            }

            var total = await consulta.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var reportes = await consulta
                .OrderByDescending(r => r.FechaReporte)
                .ThenByDescending(r => r.IdReporte)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(r => new ReporteOfertaDto
                {
                    IdReporte = r.IdReporte,
                    IdUsuario = r.IdUsuario,
                    IdOferta = r.IdOferta,
                    Cargo = r.Oferta.Cargo,
                    Empresa =
                        r.Oferta.Empresa.NombreComercial,
                    Motivo = r.Motivo,
                    FechaReporte = r.FechaReporte,
                    EstadoOferta = r.Oferta.Estado
                })
                .ToListAsync();

            return Ok(reportes);
        }

        // ==========================================
        // CONSULTAR REPORTE POR ID
        // ==========================================

        [Authorize(Roles = "Administrador de la Escuela")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var reporte = await _context.ReportesOferta
                .AsNoTracking()
                .Where(r => r.IdReporte == id)
                .Select(r => new ReporteOfertaDto
                {
                    IdReporte = r.IdReporte,
                    IdUsuario = r.IdUsuario,
                    IdOferta = r.IdOferta,
                    Cargo = r.Oferta.Cargo,
                    Empresa =
                        r.Oferta.Empresa.NombreComercial,
                    Motivo = r.Motivo,
                    FechaReporte = r.FechaReporte,
                    EstadoOferta = r.Oferta.Estado
                })
                .FirstOrDefaultAsync();

            if (reporte is null)
                return NotFound();

            return Ok(reporte);
        }
    }
}

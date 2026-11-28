
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [Authorize(Roles = "Administrador de la Escuela,Graduado")]
    [ApiController]
    [Route("api/graduados/{idGraduado:int}/proyectos")]
    public class ProyectosController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public ProyectosController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // COMPROBAR PROPIEDAD DEL PERFIL
        // ==========================================

        private async Task<bool> PuedeAcceder(int idGraduado)
        {
            if (User.IsInRole("Administrador de la Escuela"))
                return true;

            if (!User.IsInRole("Graduado"))
                return false;

            if (!int.TryParse(
                User.FindFirst("sub")?.Value,
                out int idUsuario))
            {
                return false;
            }

            return await _context.Graduados.AnyAsync(g =>
                g.IdGraduado == idGraduado &&
                g.IdUsuario == idUsuario);
        }

        // ==========================================
        // LISTAR PROYECTOS
        // ==========================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProyectoDto>>>
            Listar(int idGraduado)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            if (!await ExisteGraduado(idGraduado))
                return NotFound();

            var lista = await _context.Proyectos
                .AsNoTracking()
                .Where(p => p.IdGraduado == idGraduado)
                .OrderBy(p => p.NombreProyecto)
                .ToListAsync();

            return Ok(lista.Select(ADto));
        }

        // ==========================================
        // CONSULTAR PROYECTO
        // ==========================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProyectoDto>>
            ObtenerPorId(int idGraduado, int id)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var proyecto = await _context.Proyectos
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.IdProyecto == id &&
                    p.IdGraduado == idGraduado);

            if (proyecto is null)
                return NotFound();

            return Ok(ADto(proyecto));
        }

        // ==========================================
        // REGISTRAR PROYECTO
        // ==========================================

        [HttpPost]
        public async Task<ActionResult<ProyectoDto>>
            Crear(
                int idGraduado,
                [FromBody] ProyectoRequestDto dto)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var graduado =
                await _context.Graduados.FindAsync(idGraduado);

            if (graduado is null)
                return NotFound();

            var proyecto = new Proyecto
            {
                IdGraduado = idGraduado
            };

            Aplicar(proyecto, dto);

            _context.Proyectos.Add(proyecto);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    idGraduado,
                    id = proyecto.IdProyecto
                },
                ADto(proyecto));
        }

        // ==========================================
        // ACTUALIZAR PROYECTO
        // ==========================================

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProyectoDto>>
            Actualizar(
                int idGraduado,
                int id,
                [FromBody] ProyectoRequestDto dto)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var proyecto = await _context.Proyectos
                .FirstOrDefaultAsync(p =>
                    p.IdProyecto == id &&
                    p.IdGraduado == idGraduado);

            if (proyecto is null)
                return NotFound();

            var graduado =
                await _context.Graduados.FindAsync(idGraduado);

            if (graduado is null)
                return NotFound();

            Aplicar(proyecto, dto);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            return Ok(ADto(proyecto));
        }

        // ==========================================
        // ELIMINAR PROYECTO
        // ==========================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            Eliminar(int idGraduado, int id)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var proyecto = await _context.Proyectos
                .FirstOrDefaultAsync(p =>
                    p.IdProyecto == id &&
                    p.IdGraduado == idGraduado);

            if (proyecto is null)
                return NotFound();

            var graduado =
                await _context.Graduados.FindAsync(idGraduado);

            if (graduado is null)
                return NotFound();

            _context.Proyectos.Remove(proyecto);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================

        private Task<bool> ExisteGraduado(int idGraduado) =>
            _context.Graduados.AnyAsync(g =>
                g.IdGraduado == idGraduado);

        private static string? NullSiVacio(string? texto) =>
            string.IsNullOrWhiteSpace(texto)
                ? null
                : texto.Trim();

        private static void Aplicar(
            Proyecto proyecto,
            ProyectoRequestDto dto)
        {
            proyecto.NombreProyecto =
                dto.NombreProyecto.Trim();

            proyecto.Descripcion =
                NullSiVacio(dto.Descripcion);

            proyecto.TecnologiasUtilizadas =
                NullSiVacio(dto.TecnologiasUtilizadas);

            proyecto.UrlProyecto =
                NullSiVacio(dto.UrlProyecto);
        }

        private static ProyectoDto ADto(Proyecto p) => new()
        {
            IdProyecto = p.IdProyecto,
            IdGraduado = p.IdGraduado,
            NombreProyecto = p.NombreProyecto,
            Descripcion = p.Descripcion,
            TecnologiasUtilizadas = p.TecnologiasUtilizadas,
            UrlProyecto = p.UrlProyecto
        };

        private static DateTime Ahora() =>
            DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);
    }
}

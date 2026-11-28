
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
    [Route("api/graduados/{idGraduado:int}/experiencias")]
    public class ExperienciasController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public ExperienciasController(
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
        // LISTAR EXPERIENCIAS
        // ==========================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExperienciaDto>>>
            Listar(int idGraduado)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            if (!await ExisteGraduado(idGraduado))
                return NotFound(new
                {
                    mensaje = "No existe el graduado."
                });

            var lista = await _context.ExperienciasLaborales
                .AsNoTracking()
                .Where(e => e.IdGraduado == idGraduado)
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();

            return Ok(lista.Select(ADto));
        }

        // ==========================================
        // CONSULTAR EXPERIENCIA
        // ==========================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExperienciaDto>>
            ObtenerPorId(int idGraduado, int id)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var experiencia =
                await _context.ExperienciasLaborales
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e =>
                        e.IdExperiencia == id &&
                        e.IdGraduado == idGraduado);

            if (experiencia is null)
                return NotFound(new
                {
                    mensaje = "No existe la experiencia."
                });

            return Ok(ADto(experiencia));
        }

        // ==========================================
        // REGISTRAR EXPERIENCIA
        // ==========================================

        [HttpPost]
        public async Task<ActionResult<ExperienciaDto>>
            Crear(
                int idGraduado,
                [FromBody] ExperienciaRequestDto dto)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var graduado =
                await _context.Graduados.FindAsync(idGraduado);

            if (graduado is null)
                return NotFound();

            var experiencia = new ExperienciaLaboral
            {
                IdGraduado = idGraduado
            };

            Aplicar(experiencia, dto);

            _context.ExperienciasLaborales.Add(experiencia);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    idGraduado,
                    id = experiencia.IdExperiencia
                },
                ADto(experiencia));
        }

        // ==========================================
        // ACTUALIZAR EXPERIENCIA
        // ==========================================

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ExperienciaDto>>
            Actualizar(
                int idGraduado,
                int id,
                [FromBody] ExperienciaRequestDto dto)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var experiencia =
                await _context.ExperienciasLaborales
                    .FirstOrDefaultAsync(e =>
                        e.IdExperiencia == id &&
                        e.IdGraduado == idGraduado);

            if (experiencia is null)
                return NotFound();

            var graduado =
                await _context.Graduados.FindAsync(idGraduado);

            if (graduado is null)
                return NotFound();

            Aplicar(experiencia, dto);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            return Ok(ADto(experiencia));
        }

        // ==========================================
        // ELIMINAR EXPERIENCIA
        // ==========================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            Eliminar(int idGraduado, int id)
        {
            if (!await PuedeAcceder(idGraduado))
                return Forbid();

            var experiencia =
                await _context.ExperienciasLaborales
                    .FirstOrDefaultAsync(e =>
                        e.IdExperiencia == id &&
                        e.IdGraduado == idGraduado);

            if (experiencia is null)
                return NotFound();

            var graduado =
                await _context.Graduados.FindAsync(idGraduado);

            if (graduado is null)
                return NotFound();

            _context.ExperienciasLaborales.Remove(experiencia);

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

        private static void Aplicar(
            ExperienciaLaboral experiencia,
            ExperienciaRequestDto dto)
        {
            experiencia.Empresa = dto.Empresa.Trim();
            experiencia.Cargo = dto.Cargo.Trim();

            experiencia.Descripcion =
                dto.Descripcion.Trim();

            experiencia.FechaInicio =
                dto.FechaInicio!.Value;

            experiencia.FechaFin = dto.FechaFin;
        }

        private static ExperienciaDto ADto(
            ExperienciaLaboral e) => new()
        {
            IdExperiencia = e.IdExperiencia,
            IdGraduado = e.IdGraduado,
            Empresa = e.Empresa,
            Cargo = e.Cargo,
            Descripcion = e.Descripcion,
            FechaInicio = e.FechaInicio,
            FechaFin = e.FechaFin
        };

        private static DateTime Ahora() =>
            DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);
    }
}

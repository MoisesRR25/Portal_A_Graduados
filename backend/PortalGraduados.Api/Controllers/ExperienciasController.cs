using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/graduados/{idGraduado:int}/experiencias")]
    public class ExperienciasController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public ExperienciasController(PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // GET /api/graduados/1/experiencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExperienciaDto>>> Listar(int idGraduado)
        {
            if (!await ExisteGraduado(idGraduado))
                return NotFound(new { mensaje = $"No existe el graduado con id {idGraduado}." });

            var lista = await _context.ExperienciasLaborales.AsNoTracking()
                .Where(e => e.IdGraduado == idGraduado)
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();

            return Ok(lista.Select(ADto));
        }

        // GET /api/graduados/1/experiencias/3
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExperienciaDto>> ObtenerPorId(int idGraduado, int id)
        {
            var experiencia = await _context.ExperienciasLaborales.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdExperiencia == id && e.IdGraduado == idGraduado);

            if (experiencia is null)
                return NotFound(new { mensaje = $"El graduado {idGraduado} no tiene la experiencia {id}." });

            return Ok(ADto(experiencia));
        }

        // POST /api/graduados/1/experiencias
        [HttpPost]
        public async Task<ActionResult<ExperienciaDto>> Crear(int idGraduado, [FromBody] ExperienciaRequestDto dto)
        {
            if (!await ExisteGraduado(idGraduado))
                return NotFound(new { mensaje = $"No existe el graduado con id {idGraduado}." });

            var experiencia = new ExperienciaLaboral { IdGraduado = idGraduado };
            Aplicar(experiencia, dto);

            _context.ExperienciasLaborales.Add(experiencia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId),
                new { idGraduado, id = experiencia.IdExperiencia }, ADto(experiencia));
        }

        // PUT /api/graduados/1/experiencias/3
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ExperienciaDto>> Actualizar(int idGraduado, int id, [FromBody] ExperienciaRequestDto dto)
        {
            var experiencia = await _context.ExperienciasLaborales
                .FirstOrDefaultAsync(e => e.IdExperiencia == id && e.IdGraduado == idGraduado);

            if (experiencia is null)
                return NotFound(new { mensaje = $"El graduado {idGraduado} no tiene la experiencia {id}." });

            Aplicar(experiencia, dto);
            await _context.SaveChangesAsync();

            return Ok(ADto(experiencia));
        }

        // DELETE /api/graduados/1/experiencias/3
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int idGraduado, int id)
        {
            var experiencia = await _context.ExperienciasLaborales
                .FirstOrDefaultAsync(e => e.IdExperiencia == id && e.IdGraduado == idGraduado);

            if (experiencia is null)
                return NotFound(new { mensaje = $"El graduado {idGraduado} no tiene la experiencia {id}." });

            _context.ExperienciasLaborales.Remove(experiencia);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> ExisteGraduado(int idGraduado) =>
            _context.Graduados.AnyAsync(g => g.IdGraduado == idGraduado);

        private static void Aplicar(ExperienciaLaboral experiencia, ExperienciaRequestDto dto)
        {
            experiencia.Empresa = dto.Empresa.Trim();
            experiencia.Cargo = dto.Cargo.Trim();
            experiencia.Descripcion = dto.Descripcion.Trim();
            experiencia.FechaInicio = dto.FechaInicio!.Value;
            experiencia.FechaFin = dto.FechaFin;
        }

        private static ExperienciaDto ADto(ExperienciaLaboral e) => new()
        {
            IdExperiencia = e.IdExperiencia,
            IdGraduado = e.IdGraduado,
            Empresa = e.Empresa,
            Cargo = e.Cargo,
            Descripcion = e.Descripcion,
            FechaInicio = e.FechaInicio,
            FechaFin = e.FechaFin
        };
    }
}
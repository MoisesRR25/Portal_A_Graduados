using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabilidadesController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public HabilidadesController(PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // GET /api/habilidades?categoria=Backend
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabilidadDto>>> Listar([FromQuery] string? categoria)
        {
            var consulta = _context.Habilidades.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var cat = categoria.Trim();
                consulta = consulta.Where(h => h.Categoria == cat);
            }

            var lista = await consulta
                .OrderBy(h => h.NombreHabilidad)
                .Select(h => new HabilidadDto
                {
                    IdHabilidad = h.IdHabilidad,
                    NombreHabilidad = h.NombreHabilidad,
                    Categoria = h.Categoria
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/habilidades/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HabilidadDto>> ObtenerPorId(int id)
        {
            var habilidad = await _context.Habilidades
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHabilidad == id);

            if (habilidad is null)
                return NotFound(new { mensaje = $"No existe la habilidad con id {id}." });

            return Ok(ADto(habilidad));
        }

        // POST /api/habilidades
        [HttpPost]
        public async Task<ActionResult<HabilidadDto>> Crear([FromBody] HabilidadRequestDto dto)
        {
            var nombre = dto.NombreHabilidad.Trim();

            if (await _context.Habilidades.AnyAsync(h => h.NombreHabilidad == nombre))
                return Conflict(new { mensaje = $"Ya existe una habilidad llamada '{nombre}'." });

            var habilidad = new Habilidad
            {
                NombreHabilidad = nombre,
                Categoria = dto.Categoria.Trim()
            };

            _context.Habilidades.Add(habilidad);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Conflict(new { mensaje = $"Ya existe una habilidad llamada '{nombre}'." });
            }

            return CreatedAtAction(nameof(ObtenerPorId),
                new { id = habilidad.IdHabilidad }, ADto(habilidad));
        }

        // PUT /api/habilidades/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<HabilidadDto>> Actualizar(int id, [FromBody] HabilidadRequestDto dto)
        {
            var habilidad = await _context.Habilidades.FindAsync(id);

            if (habilidad is null)
                return NotFound(new { mensaje = $"No existe la habilidad con id {id}." });

            var nombre = dto.NombreHabilidad.Trim();

            if (await _context.Habilidades.AnyAsync(h => h.NombreHabilidad == nombre && h.IdHabilidad != id))
                return Conflict(new { mensaje = $"Ya existe otra habilidad llamada '{nombre}'." });

            habilidad.NombreHabilidad = nombre;
            habilidad.Categoria = dto.Categoria.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Conflict(new { mensaje = $"Ya existe otra habilidad llamada '{nombre}'." });
            }

            return Ok(ADto(habilidad));
        }

        // DELETE /api/habilidades/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var habilidad = await _context.Habilidades.FindAsync(id);

            if (habilidad is null)
                return NotFound(new { mensaje = $"No existe la habilidad con id {id}." });

            // Las claves foráneas usan Restrict: no se puede borrar si está en uso
            var enUso = await _context.GraduadoHabilidades.AnyAsync(gh => gh.IdHabilidad == id)
                     || await _context.OfertaHabilidades.AnyAsync(oh => oh.IdHabilidad == id);

            if (enUso)
                return Conflict(new { mensaje = "La habilidad está asociada a graduados u ofertas y no se puede eliminar." });

            _context.Habilidades.Remove(habilidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static HabilidadDto ADto(Habilidad h) => new()
        {
            IdHabilidad = h.IdHabilidad,
            NombreHabilidad = h.NombreHabilidad,
            Categoria = h.Categoria
        };
    }
}

using Microsoft.AspNetCore.Authorization;
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
        private const string Administrador =
            "Administrador de la Escuela";

        private readonly PortalGraduadosDbContext _context;

        public HabilidadesController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTAR CATÁLOGO DE HABILIDADES
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado,Empresa")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabilidadDto>>>
            Listar(
                [FromQuery] string? categoria,
                [FromQuery] string? busqueda)
        {
            var consulta = _context.Habilidades
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var cat = categoria.Trim().ToLower();

                consulta = consulta.Where(h =>
                    h.Categoria.ToLower() == cat);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim().ToLower();

                consulta = consulta.Where(h =>
                    h.NombreHabilidad.ToLower()
                        .Contains(texto));
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

        // ==========================================
        // CONSULTAR HABILIDAD
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado,Empresa")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HabilidadDto>>
            ObtenerPorId(int id)
        {
            var habilidad = await _context.Habilidades
                .AsNoTracking()
                .FirstOrDefaultAsync(h =>
                    h.IdHabilidad == id);

            if (habilidad is null)
                return NotFound(new
                {
                    mensaje = "No existe la habilidad."
                });

            return Ok(ADto(habilidad));
        }

        // ==========================================
        // CREAR HABILIDAD
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpPost]
        public async Task<ActionResult<HabilidadDto>>
            Crear([FromBody] HabilidadRequestDto dto)
        {
            var nombre = dto.NombreHabilidad.Trim();

            var nombreNormalizado = nombre.ToLower();

            if (await _context.Habilidades.AnyAsync(h =>
                h.NombreHabilidad.ToLower() ==
                nombreNormalizado))
            {
                return Conflict(new
                {
                    mensaje =
                        $"Ya existe una habilidad llamada '{nombre}'."
                });
            }

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
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                return Conflict(new
                {
                    mensaje = "La habilidad ya existe."
                });
            }

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = habilidad.IdHabilidad },
                ADto(habilidad));
        }

        // ==========================================
        // ACTUALIZAR HABILIDAD
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<HabilidadDto>>
            Actualizar(
                int id,
                [FromBody] HabilidadRequestDto dto)
        {
            var habilidad =
                await _context.Habilidades.FindAsync(id);

            if (habilidad is null)
                return NotFound();

            var nombre = dto.NombreHabilidad.Trim();

            var nombreNormalizado = nombre.ToLower();

            if (await _context.Habilidades.AnyAsync(h =>
                h.NombreHabilidad.ToLower() ==
                    nombreNormalizado &&
                h.IdHabilidad != id))
            {
                return Conflict(new
                {
                    mensaje =
                        "Ya existe otra habilidad con ese nombre."
                });
            }

            habilidad.NombreHabilidad = nombre;

            habilidad.Categoria = dto.Categoria.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                return Conflict(new
                {
                    mensaje = "La habilidad ya existe."
                });
            }

            return Ok(ADto(habilidad));
        }

        // ==========================================
        // ELIMINAR HABILIDAD
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var habilidad =
                await _context.Habilidades.FindAsync(id);

            if (habilidad is null)
                return NotFound();

            var enUso =
                await _context.GraduadoHabilidades.AnyAsync(
                    gh => gh.IdHabilidad == id)
                ||
                await _context.OfertaHabilidades.AnyAsync(
                    oh => oh.IdHabilidad == id);

            if (enUso)
            {
                return Conflict(new
                {
                    mensaje =
                        "La habilidad está asociada a graduados u ofertas."
                });
            }

            _context.Habilidades.Remove(habilidad);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState:
                        PostgresErrorCodes.ForeignKeyViolation
                })
            {
                return Conflict(new
                {
                    mensaje =
                        "La habilidad tiene registros asociados."
                });
            }

            return NoContent();
        }

        // ==========================================
        // CONVERTIR ENTIDAD A DTO
        // ==========================================

        private static HabilidadDto ADto(
            Habilidad h) => new()
        {
            IdHabilidad = h.IdHabilidad,
            NombreHabilidad = h.NombreHabilidad,
            Categoria = h.Categoria
        };
    }
}

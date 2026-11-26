using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/graduados/{idGraduado:int}/proyectos")]
    public class ProyectosController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public ProyectosController(PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // GET /api/graduados/1/proyectos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProyectoDto>>> Listar(int idGraduado)
        {
            if (!await ExisteGraduado(idGraduado))
                return NotFound(new { mensaje = $"No existe el graduado con id {idGraduado}." });

            var lista = await _context.Proyectos.AsNoTracking()
                .Where(p => p.IdGraduado == idGraduado)
                .OrderBy(p => p.NombreProyecto)
                .ToListAsync();

            return Ok(lista.Select(ADto));
        }

        // GET /api/graduados/1/proyectos/2
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProyectoDto>> ObtenerPorId(int idGraduado, int id)
        {
            var proyecto = await _context.Proyectos.AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProyecto == id && p.IdGraduado == idGraduado);

            if (proyecto is null)
                return NotFound(new { mensaje = $"El graduado {idGraduado} no tiene el proyecto {id}." });

            return Ok(ADto(proyecto));
        }

        // POST /api/graduados/1/proyectos
        [HttpPost]
        public async Task<ActionResult<ProyectoDto>> Crear(int idGraduado, [FromBody] ProyectoRequestDto dto)
        {
            if (!await ExisteGraduado(idGraduado))
                return NotFound(new { mensaje = $"No existe el graduado con id {idGraduado}." });

            var proyecto = new Proyecto { IdGraduado = idGraduado };
            Aplicar(proyecto, dto);

            _context.Proyectos.Add(proyecto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId),
                new { idGraduado, id = proyecto.IdProyecto }, ADto(proyecto));
        }

        // PUT /api/graduados/1/proyectos/2
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProyectoDto>> Actualizar(int idGraduado, int id, [FromBody] ProyectoRequestDto dto)
        {
            var proyecto = await _context.Proyectos
                .FirstOrDefaultAsync(p => p.IdProyecto == id && p.IdGraduado == idGraduado);

            if (proyecto is null)
                return NotFound(new { mensaje = $"El graduado {idGraduado} no tiene el proyecto {id}." });

            Aplicar(proyecto, dto);
            await _context.SaveChangesAsync();

            return Ok(ADto(proyecto));
        }

        // DELETE /api/graduados/1/proyectos/2
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int idGraduado, int id)
        {
            var proyecto = await _context.Proyectos
                .FirstOrDefaultAsync(p => p.IdProyecto == id && p.IdGraduado == idGraduado);

            if (proyecto is null)
                return NotFound(new { mensaje = $"El graduado {idGraduado} no tiene el proyecto {id}." });

            _context.Proyectos.Remove(proyecto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> ExisteGraduado(int idGraduado) =>
            _context.Graduados.AnyAsync(g => g.IdGraduado == idGraduado);

        private static string? NullSiVacio(string? texto) =>
            string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

        private static void Aplicar(Proyecto proyecto, ProyectoRequestDto dto)
        {
            proyecto.NombreProyecto = dto.NombreProyecto.Trim();
            proyecto.Descripcion = NullSiVacio(dto.Descripcion);
            proyecto.TecnologiasUtilizadas = NullSiVacio(dto.TecnologiasUtilizadas);
            proyecto.UrlProyecto = NullSiVacio(dto.UrlProyecto);
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
    }
}
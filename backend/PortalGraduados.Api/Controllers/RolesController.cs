using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public RolesController(PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // GET /api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolDto>>> Listar()
        {
            var lista = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.NombreRol)
                .Select(r => new RolDto
                {
                    IdRol = r.IdRol,
                    NombreRol = r.NombreRol,
                    Descripcion = r.Descripcion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/roles/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RolDto>> ObtenerPorId(int id)
        {
            var rol = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol is null)
                return NotFound(new { mensaje = $"No existe el rol con id {id}." });

            return Ok(ADto(rol));
        }

        // POST /api/roles
        [HttpPost]
        public async Task<ActionResult<RolDto>> Crear([FromBody] RolRequestDto dto)
        {
            var nombre = dto.NombreRol.Trim();

            if (await _context.Roles.AnyAsync(r => r.NombreRol.ToLower() == nombre.ToLower()))
                return Conflict(new { mensaje = $"Ya existe un rol llamado '{nombre}'." });

            var rol = new Rol
            {
                NombreRol = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim()
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = rol.IdRol }, ADto(rol));
        }

        // PUT /api/roles/1
        [HttpPut("{id:int}")]
        public async Task<ActionResult<RolDto>> Actualizar(int id, [FromBody] RolRequestDto dto)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol is null)
                return NotFound(new { mensaje = $"No existe el rol con id {id}." });

            var nombre = dto.NombreRol.Trim();

            if (await _context.Roles.AnyAsync(r => r.NombreRol.ToLower() == nombre.ToLower() && r.IdRol != id))
                return Conflict(new { mensaje = $"Ya existe otro rol llamado '{nombre}'." });

            rol.NombreRol = nombre;
            rol.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

            await _context.SaveChangesAsync();
            return Ok(ADto(rol));
        }

        // DELETE /api/roles/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol is null)
                return NotFound(new { mensaje = $"No existe el rol con id {id}." });

            if (await _context.Usuarios.AnyAsync(u => u.IdRol == id))
                return Conflict(new { mensaje = "El rol está asignado a usuarios y no se puede eliminar." });

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static RolDto ADto(Rol r) => new()
        {
            IdRol = r.IdRol,
            NombreRol = r.NombreRol,
            Descripcion = r.Descripcion
        };
    }
}
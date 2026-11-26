using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
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
    public class GraduadosController : ControllerBase
    {
        private const string NombreRolGraduado = "Graduado";

        private readonly PortalGraduadosDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public GraduadosController(PortalGraduadosDbContext context, IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        private static readonly Expression<Func<Graduado, GraduadoDto>> Proyeccion = g => new GraduadoDto
        {
            IdGraduado = g.IdGraduado,
            IdUsuario = g.IdUsuario,
            Correo = g.Usuario.Correo,
            Nombres = g.Nombres,
            Apellidos = g.Apellidos,
            Telefono = g.Telefono,
            AnioGraduacion = g.AnioGraduacion,
            EstadoAcademico = g.EstadoAcademico,
            EstadoLaboral = g.EstadoLaboral,
            AreaTrabajo = g.AreaTrabajo,
            Ubicacion = g.Ubicacion,
            CvAdjunto = g.CvAdjunto,
            PortafolioEnlace = g.PortafolioEnlace,
            Verificado = g.Verificado,
            Consentimiento = g.Consentimiento,
            FechaUltimaActualizacion = g.FechaUltimaActualizacion
        };

        // GET /api/graduados?estadoLaboral=Empleado&pagina=1&tamano=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GraduadoDto>>> Listar(
            [FromQuery] string? areaTrabajo,
            [FromQuery] string? estadoLaboral,
            [FromQuery] int? anioGraduacion,
            [FromQuery] bool? verificado,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "pagina debe ser 1 o mayor y tamano debe estar entre 1 y 100." });

            var consulta = _context.Graduados.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(areaTrabajo))
            {
                var area = areaTrabajo.Trim().ToLower();
                consulta = consulta.Where(g => g.AreaTrabajo.ToLower() == area);
            }

            if (!string.IsNullOrWhiteSpace(estadoLaboral))
            {
                var estado = estadoLaboral.Trim().ToLower();
                consulta = consulta.Where(g => g.EstadoLaboral.ToLower() == estado);
            }

            if (anioGraduacion.HasValue)
                consulta = consulta.Where(g => g.AnioGraduacion == anioGraduacion.Value);

            if (verificado.HasValue)
                consulta = consulta.Where(g => g.Verificado == verificado.Value);

            var total = await consulta.CountAsync();
            Response.Headers["X-Total-Count"] = total.ToString();

            var lista = await consulta
                .OrderBy(g => g.Apellidos)
                .ThenBy(g => g.Nombres)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/graduados/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GraduadoDetalleDto>> ObtenerPorId(int id)
        {
            var graduado = await _context.Graduados.AsNoTracking()
                .Where(g => g.IdGraduado == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (graduado is null)
                return NotFound(new { mensaje = $"No existe el graduado con id {id}." });

            var habilidades = await ConsultarHabilidades(id);

            return Ok(new GraduadoDetalleDto { Graduado = graduado, Habilidades = habilidades });
        }

        // POST /api/graduados  (registro: usuario + perfil en una transaccion)
        [HttpPost]
        public async Task<ActionResult<GraduadoDto>> Registrar([FromBody] GraduadoRegistroDto dto)
        {
            if (!dto.Consentimiento)
                return BadRequest(new { mensaje = "Debe aceptar el consentimiento para registrar el perfil." });

            var correo = dto.Correo.Trim().ToLowerInvariant();

            var rol = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.NombreRol == NombreRolGraduado);

            if (rol is null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = $"No existe el rol '{NombreRolGraduado}'. Cree primero los roles base." });

            if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
                return Conflict(new { mensaje = "Ya existe un usuario con ese correo." });

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso 1: crear el usuario
                var usuario = new Usuario
                {
                    IdRol = rol.IdRol,
                    Correo = correo,
                    EstadoCuenta = true
                };
                usuario.ContrasenaHash = _hasher.HashPassword(usuario, dto.Contrasena);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Paso 2: crear el perfil del graduado (sin verificar)
                var graduado = new Graduado
                {
                    IdUsuario = usuario.IdUsuario,
                    Verificado = false,
                    Consentimiento = true
                };
                AplicarPerfil(graduado, dto);
                graduado.FechaUltimaActualizacion = Ahora();

                _context.Graduados.Add(graduado);
                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                var respuesta = await _context.Graduados.AsNoTracking()
                    .Where(g => g.IdGraduado == graduado.IdGraduado)
                    .Select(Proyeccion)
                    .FirstAsync();

                return CreatedAtAction(nameof(ObtenerPorId), new { id = graduado.IdGraduado }, respuesta);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                await transaccion.RollbackAsync();
                return Conflict(new { mensaje = "Ya existe un usuario con ese correo." });
            }
            // Cualquier otra excepcion sale sin CommitAsync: la transaccion se revierte.
        }

        // PUT /api/graduados/1
        [HttpPut("{id:int}")]
        public async Task<ActionResult<GraduadoDto>> Actualizar(int id, [FromBody] GraduadoActualizarDto dto)
        {
            var graduado = await _context.Graduados.FindAsync(id);

            if (graduado is null)
                return NotFound(new { mensaje = $"No existe el graduado con id {id}." });

            AplicarPerfil(graduado, dto);
            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            var respuesta = await _context.Graduados.AsNoTracking()
                .Where(g => g.IdGraduado == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // PUT /api/graduados/1/verificacion
        [HttpPut("{id:int}/verificacion")]
        public async Task<ActionResult<GraduadoDto>> CambiarVerificacion(int id, [FromBody] VerificacionGraduadoDto dto)
        {
            var graduado = await _context.Graduados.FindAsync(id);

            if (graduado is null)
                return NotFound(new { mensaje = $"No existe el graduado con id {id}." });

            graduado.Verificado = dto.Verificado!.Value;
            await _context.SaveChangesAsync();

            var respuesta = await _context.Graduados.AsNoTracking()
                .Where(g => g.IdGraduado == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // GET /api/graduados/1/habilidades
        [HttpGet("{id:int}/habilidades")]
        public async Task<ActionResult<IEnumerable<HabilidadDto>>> ListarHabilidades(int id)
        {
            if (!await _context.Graduados.AnyAsync(g => g.IdGraduado == id))
                return NotFound(new { mensaje = $"No existe el graduado con id {id}." });

            return Ok(await ConsultarHabilidades(id));
        }

        // PUT /api/graduados/1/habilidades/3  (idempotente)
        [HttpPut("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> AsignarHabilidad(int id, int idHabilidad)
        {
            if (!await _context.Graduados.AnyAsync(g => g.IdGraduado == id))
                return NotFound(new { mensaje = $"No existe el graduado con id {id}." });

            if (!await _context.Habilidades.AnyAsync(h => h.IdHabilidad == idHabilidad))
                return NotFound(new { mensaje = $"No existe la habilidad con id {idHabilidad}." });

            if (await _context.GraduadoHabilidades.AnyAsync(gh => gh.IdGraduado == id && gh.IdHabilidad == idHabilidad))
                return NoContent();

            _context.GraduadoHabilidades.Add(new GraduadoHabilidad
            {
                IdGraduado = id,
                IdHabilidad = idHabilidad
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Otra peticion la asigno al mismo tiempo: el resultado es el mismo
            }

            return NoContent();
        }

        // DELETE /api/graduados/1/habilidades/3
        [HttpDelete("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> QuitarHabilidad(int id, int idHabilidad)
        {
            var asignacion = await _context.GraduadoHabilidades.FindAsync(id, idHabilidad);

            if (asignacion is null)
                return NotFound(new { mensaje = "El graduado no tiene asignada esa habilidad." });

            _context.GraduadoHabilidades.Remove(asignacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<List<HabilidadDto>> ConsultarHabilidades(int idGraduado) =>
            _context.GraduadoHabilidades.AsNoTracking()
                .Where(gh => gh.IdGraduado == idGraduado)
                .OrderBy(gh => gh.Habilidad.NombreHabilidad)
                .Select(gh => new HabilidadDto
                {
                    IdHabilidad = gh.IdHabilidad,
                    NombreHabilidad = gh.Habilidad.NombreHabilidad,
                    Categoria = gh.Habilidad.Categoria
                })
                .ToListAsync();

        private static void AplicarPerfil(Graduado graduado, GraduadoPerfilDto dto)
        {
            graduado.Nombres = dto.Nombres.Trim();
            graduado.Apellidos = dto.Apellidos.Trim();
            graduado.Telefono = dto.Telefono.Trim();
            graduado.AnioGraduacion = dto.AnioGraduacion;
            graduado.EstadoAcademico = dto.EstadoAcademico.Trim();
            graduado.EstadoLaboral = dto.EstadoLaboral.Trim();
            graduado.AreaTrabajo = dto.AreaTrabajo.Trim();
            graduado.Ubicacion = dto.Ubicacion.Trim();
            graduado.CvAdjunto = NullSiVacio(dto.CvAdjunto);
            graduado.PortafolioEnlace = NullSiVacio(dto.PortafolioEnlace);
        }

        private static string? NullSiVacio(string? texto) =>
            string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

        // Npgsql no acepta DateTime con Kind=Utc en columnas "timestamp without time zone"
        private static DateTime Ahora() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }
}
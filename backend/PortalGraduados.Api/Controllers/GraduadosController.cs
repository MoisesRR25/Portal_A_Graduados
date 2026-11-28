
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        private const string Administrador = "Administrador de la Escuela";

        private readonly PortalGraduadosDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public GraduadosController(
            PortalGraduadosDbContext context,
            IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // ==========================================
        // PROYECCIÓN DEL PERFIL COMPLETO
        // ==========================================

        private static readonly Expression<Func<Graduado, GraduadoDto>>
            Proyeccion = g => new GraduadoDto
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

        // ==========================================
        // COMPROBAR PROPIEDAD DEL PERFIL
        // ==========================================

        private async Task<bool> PuedeAccederPerfil(int id)
        {
            if (User.IsInRole(Administrador))
                return true;

            if (!User.IsInRole(NombreRolGraduado))
                return false;

            if (!int.TryParse(
                User.FindFirst("sub")?.Value,
                out int idUsuario))
            {
                return false;
            }

            return await _context.Graduados.AnyAsync(g =>
                g.IdGraduado == id &&
                g.IdUsuario == idUsuario);
        }

        // ==========================================
        // LISTAR PERFILES COMPLETOS
        // Exclusivo del administrador.
        // ==========================================

        [Authorize(Roles = Administrador)]
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
            {
                return BadRequest(new
                {
                    mensaje = "Página o tamaño inválidos."
                });
            }

            var consulta = _context.Graduados.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(areaTrabajo))
            {
                var area = areaTrabajo.Trim().ToLower();

                consulta = consulta.Where(g =>
                    g.AreaTrabajo.ToLower() == area);
            }

            if (!string.IsNullOrWhiteSpace(estadoLaboral))
            {
                var estado = estadoLaboral.Trim().ToLower();

                consulta = consulta.Where(g =>
                    g.EstadoLaboral.ToLower() == estado);
            }

            if (anioGraduacion.HasValue)
            {
                consulta = consulta.Where(g =>
                    g.AnioGraduacion == anioGraduacion.Value);
            }

            if (verificado.HasValue)
            {
                consulta = consulta.Where(g =>
                    g.Verificado == verificado.Value);
            }

            var total = await consulta.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var lista = await consulta
                .OrderBy(g => g.Apellidos)
                .ThenBy(g => g.Nombres)
                .ThenBy(g => g.IdGraduado)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // ==========================================
        // RF-10: BUSCAR PERFILES PROFESIONALES
        // Empresa y administrador.
        // No devuelve correo, teléfono ni CV.
        // ==========================================

        [Authorize(Roles = Administrador + ",Empresa")]
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar(
            [FromQuery] string? busqueda,
            [FromQuery] string? areaTrabajo,
            [FromQuery] string? ubicacion,
            [FromQuery] string? habilidad,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
            {
                return BadRequest(new
                {
                    mensaje = "Página o tamaño inválidos."
                });
            }

            // Solo perfiles activos y verificados.
            // Los pendientes no aparecen en la búsqueda.

            var consulta = _context.Graduados
                .AsNoTracking()
                .Where(g =>
                    g.Verificado &&
                    g.Consentimiento &&
                    g.Usuario.EstadoCuenta);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim().ToLower();

                consulta = consulta.Where(g =>
                    (g.Nombres + " " + g.Apellidos)
                        .ToLower().Contains(texto) ||
                    g.AreaTrabajo.ToLower().Contains(texto) ||
                    g.Ubicacion.ToLower().Contains(texto) ||
                    _context.GraduadoHabilidades.Any(gh =>
                        gh.IdGraduado == g.IdGraduado &&
                        gh.Habilidad.NombreHabilidad
                            .ToLower().Contains(texto)));
            }

            if (!string.IsNullOrWhiteSpace(areaTrabajo))
            {
                var area = areaTrabajo.Trim().ToLower();

                consulta = consulta.Where(g =>
                    g.AreaTrabajo.ToLower() == area);
            }

            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                var lugar = ubicacion.Trim().ToLower();

                consulta = consulta.Where(g =>
                    g.Ubicacion.ToLower().Contains(lugar));
            }

            if (!string.IsNullOrWhiteSpace(habilidad))
            {
                var nombre = habilidad.Trim().ToLower();

                consulta = consulta.Where(g =>
                    _context.GraduadoHabilidades.Any(gh =>
                        gh.IdGraduado == g.IdGraduado &&
                        gh.Habilidad.NombreHabilidad
                            .ToLower().Contains(nombre)));
            }

            var total = await consulta.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var perfiles = await consulta
                .OrderBy(g => g.Apellidos)
                .ThenBy(g => g.Nombres)
                .ThenBy(g => g.IdGraduado)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(g => new
                {
                    g.IdGraduado,
                    Nombre = g.Nombres + " " + g.Apellidos,
                    g.AreaTrabajo,
                    g.Ubicacion
                })
                .ToListAsync();

            var ids = perfiles
                .Select(g => g.IdGraduado)
                .ToList();

            var habilidades = await _context.GraduadoHabilidades
                .AsNoTracking()
                .Where(gh => ids.Contains(gh.IdGraduado))
                .Select(gh => new
                {
                    gh.IdGraduado,
                    gh.Habilidad.NombreHabilidad
                })
                .ToListAsync();

            var resultado = perfiles.Select(g => new
            {
                g.IdGraduado,
                g.Nombre,
                g.AreaTrabajo,
                g.Ubicacion,

                Habilidades = habilidades
                    .Where(h =>
                        h.IdGraduado == g.IdGraduado)
                    .Select(h => h.NombreHabilidad)
                    .OrderBy(h => h)
                    .ToList()
            });

            return Ok(resultado);
        }

        // ==========================================
        // CONSULTAR PERFIL PROPIO
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GraduadoDetalleDto>>
            ObtenerPorId(int id)
        {
            if (!await PuedeAccederPerfil(id))
                return Forbid();

            var graduado = await _context.Graduados
                .AsNoTracking()
                .Where(g => g.IdGraduado == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (graduado is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el graduado con id {id}."
                });
            }

            var habilidades = await ConsultarHabilidades(id);

            return Ok(new GraduadoDetalleDto
            {
                Graduado = graduado,
                Habilidades = habilidades
            });
        }

        // ==========================================
        // RF-01: REGISTRAR GRADUADO
        // Usuario y perfil en una transacción.
        // ==========================================

        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost]
        public async Task<ActionResult<GraduadoDto>> Registrar(
            [FromBody] GraduadoRegistroDto dto)
        {
            if (!dto.Consentimiento)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe aceptar el consentimiento para registrar el perfil."
                });
            }

            var correo = dto.Correo.Trim().ToLowerInvariant();

            var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.NombreRol == NombreRolGraduado);

            if (rol is null)
            {
                return StatusCode(500, new
                {
                    mensaje = "No existe el rol Graduado."
                });
            }

            if (await _context.Usuarios.AnyAsync(u =>
                u.Correo == correo))
            {
                return Conflict(new
                {
                    mensaje = "Ya existe un usuario con ese correo."
                });
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = new Usuario
                {
                    IdRol = rol.IdRol,
                    Correo = correo,

                    // La cuenta empieza inactiva.
                    EstadoCuenta = false
                };

                usuario.ContrasenaHash =
                    _hasher.HashPassword(
                        usuario,
                        dto.Contrasena);

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();

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

                var respuesta = await _context.Graduados
                    .AsNoTracking()
                    .Where(g =>
                        g.IdGraduado == graduado.IdGraduado)
                    .Select(Proyeccion)
                    .FirstAsync();

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = graduado.IdGraduado },
                    respuesta);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                await transaccion.RollbackAsync();

                return Conflict(new
                {
                    mensaje = "Ya existe un usuario con ese correo."
                });
            }
        }

        // ==========================================
        // RF-02 Y RF-04: ACTUALIZAR PERFIL
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<GraduadoDto>> Actualizar(
            int id,
            [FromBody] GraduadoActualizarDto dto)
        {
            if (!await PuedeAccederPerfil(id))
                return Forbid();

            var graduado =
                await _context.Graduados.FindAsync(id);

            if (graduado is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el graduado con id {id}."
                });
            }

            AplicarPerfil(graduado, dto);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            var respuesta = await _context.Graduados
                .AsNoTracking()
                .Where(g => g.IdGraduado == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // RF-16: VERIFICACIÓN ADMINISTRATIVA
        // Pendiente de implementar código de contacto.
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpPut("{id:int}/verificacion")]
        public async Task<ActionResult<GraduadoDto>>
            CambiarVerificacion(
                int id,
                [FromBody] VerificacionGraduadoDto dto)
        {
            var graduado = await _context.Graduados
                .Include(g => g.Usuario)
                .FirstOrDefaultAsync(g =>
                    g.IdGraduado == id);

            if (graduado is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el graduado con id {id}."
                });
            }

            if (dto.Verificado!.Value &&
                !graduado.Consentimiento)
            {
                return Conflict(new
                {
                    mensaje = "El graduado no ha aceptado el consentimiento."
                });
            }

            graduado.Verificado = dto.Verificado.Value;

            graduado.Usuario.EstadoCuenta =
                graduado.Verificado &&
                graduado.Consentimiento;

            graduado.FechaUltimaActualizacion = Ahora();

            // Los cambios de usuario y perfil se guardan juntos.
            await _context.SaveChangesAsync();

            var respuesta = await _context.Graduados
                .AsNoTracking()
                .Where(g => g.IdGraduado == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // LISTAR HABILIDADES PROPIAS
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado")]
        [HttpGet("{id:int}/habilidades")]
        public async Task<ActionResult<IEnumerable<HabilidadDto>>>
            ListarHabilidades(int id)
        {
            if (!await PuedeAccederPerfil(id))
                return Forbid();

            if (!await _context.Graduados.AnyAsync(g =>
                g.IdGraduado == id))
            {
                return NotFound(new
                {
                    mensaje = $"No existe el graduado con id {id}."
                });
            }

            return Ok(await ConsultarHabilidades(id));
        }

        // ==========================================
        // ASIGNAR HABILIDAD AL PERFIL PROPIO
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado")]
        [HttpPut("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> AsignarHabilidad(
            int id,
            int idHabilidad)
        {
            if (!await PuedeAccederPerfil(id))
                return Forbid();

            var graduado =
                await _context.Graduados.FindAsync(id);

            if (graduado is null)
            {
                return NotFound(new
                {
                    mensaje = "No existe el graduado."
                });
            }

            if (!await _context.Habilidades.AnyAsync(h =>
                h.IdHabilidad == idHabilidad))
            {
                return NotFound(new
                {
                    mensaje = "No existe la habilidad."
                });
            }

            if (await _context.GraduadoHabilidades.AnyAsync(gh =>
                gh.IdGraduado == id &&
                gh.IdHabilidad == idHabilidad))
            {
                return NoContent();
            }

            _context.GraduadoHabilidades.Add(
                new GraduadoHabilidad
                {
                    IdGraduado = id,
                    IdHabilidad = idHabilidad
                });

            graduado.FechaUltimaActualizacion = Ahora();

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
                // Otra petición agregó la misma habilidad.
            }

            return NoContent();
        }

        // ==========================================
        // ELIMINAR HABILIDAD DEL PERFIL PROPIO
        // ==========================================

        [Authorize(Roles = Administrador + ",Graduado")]
        [HttpDelete("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> QuitarHabilidad(
            int id,
            int idHabilidad)
        {
            if (!await PuedeAccederPerfil(id))
                return Forbid();

            var asignacion =
                await _context.GraduadoHabilidades
                    .FindAsync(id, idHabilidad);

            if (asignacion is null)
            {
                return NotFound(new
                {
                    mensaje = "El graduado no tiene asignada esa habilidad."
                });
            }

            var graduado =
                await _context.Graduados.FindAsync(id);

            if (graduado is null)
                return NotFound();

            _context.GraduadoHabilidades.Remove(asignacion);

            graduado.FechaUltimaActualizacion = Ahora();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================

        private Task<List<HabilidadDto>> ConsultarHabilidades(
            int idGraduado)
        {
            return _context.GraduadoHabilidades
                .AsNoTracking()
                .Where(gh => gh.IdGraduado == idGraduado)
                .OrderBy(gh => gh.Habilidad.NombreHabilidad)
                .Select(gh => new HabilidadDto
                {
                    IdHabilidad = gh.IdHabilidad,
                    NombreHabilidad = gh.Habilidad.NombreHabilidad,
                    Categoria = gh.Habilidad.Categoria
                })
                .ToListAsync();
        }

        private static void AplicarPerfil(
            Graduado graduado,
            GraduadoPerfilDto dto)
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
            graduado.PortafolioEnlace =
                NullSiVacio(dto.PortafolioEnlace);
        }

        private static string? NullSiVacio(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? null
                : texto.Trim();
        }

        private static DateTime Ahora()
        {
            return DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);
        }
    }
}

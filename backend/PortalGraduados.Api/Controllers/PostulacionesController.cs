
using System.Linq.Expressions;
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
    public class PostulacionesController : ControllerBase
    {
        private const string Administrador =
            "Administrador de la Escuela";

        private const string Graduado = "Graduado";
        private const string Empresa = "Empresa";

        private const string Pendiente = "Pendiente";
        private const string EnRevision = "En revision";
        private const string Aceptada = "Aceptada";
        private const string Rechazada = "Rechazada";

        private const string OfertaPublicada = "Publicada";

        private static readonly Dictionary<string, string[]>
            Transiciones = new()
        {
            [Pendiente] = new[]
            {
                EnRevision,
                Rechazada
            },

            [EnRevision] = new[]
            {
                Aceptada,
                Rechazada
            },

            [Aceptada] = Array.Empty<string>(),

            [Rechazada] = Array.Empty<string>()
        };

        private readonly PortalGraduadosDbContext _context;

        public PostulacionesController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // PROYECCIÓN DE POSTULACIONES
        // ==========================================

        private static readonly
            Expression<Func<Postulacion, PostulacionDto>>
            Proyeccion = p => new PostulacionDto
        {
            IdPostulacion = p.IdPostulacion,
            IdGraduado = p.IdGraduado,

            NombreGraduado =
                p.Graduado.Nombres + " " +
                p.Graduado.Apellidos,

            IdOferta = p.IdOferta,
            CargoOferta = p.Oferta.Cargo,

            IdEmpresa = p.Oferta.IdEmpresa,

            NombreEmpresa =
                p.Oferta.Empresa.NombreComercial,

            FechaPostulacion = p.FechaPostulacion,
            CvEnviado = p.CvEnviado,
            Estado = p.Estado
        };

        // ==========================================
        // IDENTIFICAR USUARIO
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
        // POSTULACIONES PERMITIDAS POR ROL
        // ==========================================

        private IQueryable<Postulacion>
            ObtenerPostulacionesPermitidas()
        {
            var consulta =
                _context.Postulaciones.AsNoTracking();

            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return consulta.Where(p => false);

            if (User.IsInRole(Administrador))
                return consulta;

            if (User.IsInRole(Graduado))
            {
                return consulta.Where(p =>
                    p.Graduado.IdUsuario ==
                        idUsuario.Value);
            }

            if (User.IsInRole(Empresa))
            {
                return consulta.Where(p =>
                    p.Oferta.Empresa.IdUsuario ==
                        idUsuario.Value);
            }

            return consulta.Where(p => false);
        }

        // ==========================================
        // LISTAR POSTULACIONES
        // RF-19 Y RF-20
        // ==========================================

        [Authorize(Roles =
            Administrador + ",Graduado,Empresa")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostulacionDto>>>
            Listar(
                [FromQuery] int? idGraduado,
                [FromQuery] int? idOferta,
                [FromQuery] int? idEmpresa,
                [FromQuery] string? estado,
                [FromQuery] int pagina = 1,
                [FromQuery] int tamano = 20)
        {
            if (pagina < 1 ||
                tamano < 1 ||
                tamano > 100)
            {
                return BadRequest(new
                {
                    mensaje = "Pagina o tamano invalidos."
                });
            }

            var consulta =
                ObtenerPostulacionesPermitidas();

            if (idGraduado.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdGraduado == idGraduado.Value);
            }

            if (idOferta.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.IdOferta == idOferta.Value);
            }

            if (idEmpresa.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.Oferta.IdEmpresa ==
                        idEmpresa.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var valor = estado.Trim();

                consulta = consulta.Where(p =>
                    p.Estado == valor);
            }

            var total = await consulta.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var lista = await consulta
                .OrderByDescending(p =>
                    p.FechaPostulacion)
                .ThenByDescending(p =>
                    p.IdPostulacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // ==========================================
        // CONSULTAR POSTULACIÓN
        // ==========================================

        [Authorize(Roles =
            Administrador + ",Graduado,Empresa")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PostulacionDto>>
            ObtenerPorId(int id)
        {
            var postulacion =
                await ObtenerPostulacionesPermitidas()
                    .Where(p =>
                        p.IdPostulacion == id)
                    .Select(Proyeccion)
                    .FirstOrDefaultAsync();

            if (postulacion is null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Postulacion no disponible."
                });
            }

            return Ok(postulacion);
        }

        // ==========================================
        // REGISTRAR POSTULACIÓN REAL
        // RF-08
        // ==========================================

        [Authorize(Roles = Graduado)]
        [HttpPost]
        public async Task<ActionResult<PostulacionDto>>
            Postular(
                [FromBody] PostulacionCrearDto dto)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var graduado = await _context.Graduados
                .Include(g => g.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.IdUsuario == idUsuario.Value);

            if (graduado is null)
                return Forbid();

            if (dto.IdGraduado != graduado.IdGraduado)
                return Forbid();

            if (!graduado.Verificado ||
                !graduado.Consentimiento ||
                !graduado.Usuario.EstadoCuenta)
            {
                return Conflict(new
                {
                    mensaje =
                        "El perfil debe estar activo, verificado y con consentimiento."
                });
            }

            var oferta = await _context.Ofertas
                .Include(o => o.Empresa)
                .ThenInclude(e => e.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(o =>
                    o.IdOferta == dto.IdOferta);

            if (oferta is null)
            {
                return NotFound(new
                {
                    mensaje = "No existe la oferta."
                });
            }

            var ahora = Ahora();

            // VALIDAR ESTADO DE OFERTA

            if (oferta.Estado != OfertaPublicada ||
                oferta.FechaCierre <= ahora ||
                !oferta.Empresa.EstadoAprobacion ||
                !oferta.Empresa.Usuario.EstadoCuenta)
            {
                return Conflict(new
                {
                    mensaje =
                        "La oferta no esta disponible para postulaciones."
                });
            }

            // IMPEDIR POSTULACIONES DUPLICADAS

            if (await _context.Postulaciones.AnyAsync(p =>
                p.IdGraduado == graduado.IdGraduado &&
                p.IdOferta == oferta.IdOferta))
            {
                return Conflict(new
                {
                    mensaje =
                        "Ya te postulaste a esta oferta."
                });
            }

            // ==========================================
            // VALIDAR CV REAL Y PROPIEDAD
            // ==========================================

            var cv = string.IsNullOrWhiteSpace(
                dto.CvEnviado)
                ? graduado.CvAdjunto
                : dto.CvEnviado.Trim();

            if (!CvArchivos.ObtenerRuta(
                graduado.IdGraduado,
                cv,
                out _))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debes subir un CV PDF valido antes de postularte."
                });
            }

            // ==========================================
            // GUARDAR POSTULACIÓN
            // ==========================================

            var postulacion = new Postulacion
            {
                IdGraduado = graduado.IdGraduado,

                IdOferta = oferta.IdOferta,

                FechaPostulacion = ahora,

                CvEnviado = cv!,

                Estado = Pendiente
            };

            _context.Postulaciones.Add(postulacion);

            // NOTIFICAR A LA EMPRESA

            _context.Notificaciones.Add(
                new Notificacion
                {
                    IdUsuario = oferta.Empresa.IdUsuario,

                    Tipo = "Postulacion",

                    Mensaje =
                        $"{graduado.Nombres} " +
                        $"{graduado.Apellidos} " +
                        $"se postuló a tu oferta \"{oferta.Cargo}\".",

                    Fecha = ahora,

                    Leida = false
                });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState:
                        PostgresErrorCodes.UniqueViolation
                })
            {
                return Conflict(new
                {
                    mensaje =
                        "Ya te postulaste a esta oferta."
                });
            }

            var respuesta = await _context.Postulaciones
                .AsNoTracking()
                .Where(p =>
                    p.IdPostulacion ==
                        postulacion.IdPostulacion)
                .Select(Proyeccion)
                .FirstAsync();

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    id = postulacion.IdPostulacion
                },
                respuesta);
        }

        // ==========================================
        // CAMBIAR ESTADO DE CANDIDATURA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpPut("{id:int}/estado")]
        public async Task<ActionResult<PostulacionDto>>
            CambiarEstado(
                int id,
                [FromBody] CambioEstadoPostulacionDto dto)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var postulacion =
                await _context.Postulaciones
                    .Include(p => p.Graduado)
                    .Include(p => p.Oferta)
                    .ThenInclude(o => o.Empresa)
                    .FirstOrDefaultAsync(p =>
                        p.IdPostulacion == id);

            if (postulacion is null)
                return NotFound();

            if (postulacion.Oferta.Empresa.IdUsuario !=
                idUsuario.Value)
            {
                return Forbid();
            }

            if (!Transiciones.TryGetValue(
                postulacion.Estado,
                out var permitidos) ||
                !permitidos.Contains(dto.Estado))
            {
                return Conflict(new
                {
                    mensaje =
                        $"No se puede pasar de " +
                        $"'{postulacion.Estado}' " +
                        $"a '{dto.Estado}'."
                });
            }

            postulacion.Estado = dto.Estado;

            _context.Notificaciones.Add(
                new Notificacion
                {
                    IdUsuario =
                        postulacion.Graduado.IdUsuario,

                    Tipo = "Postulacion",

                    Mensaje =
                        $"Tu postulación a " +
                        $"\"{postulacion.Oferta.Cargo}\" " +
                        $"cambió a {dto.Estado}.",

                    Fecha = Ahora(),

                    Leida = false
                });

            await _context.SaveChangesAsync();

            var respuesta =
                await _context.Postulaciones
                    .AsNoTracking()
                    .Where(p =>
                        p.IdPostulacion == id)
                    .Select(Proyeccion)
                    .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // CONSULTAR CONTACTO DEL CANDIDATO
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpGet("{id:int}/contacto")]
        public async Task<IActionResult>
            ConsultarContacto(int id)
        {
            var contacto =
                await ObtenerPostulacionesPermitidas()
                    .Where(p =>
                        p.IdPostulacion == id &&
                        p.Graduado.Consentimiento &&
                        p.Graduado.Verificado &&
                        p.Graduado.Usuario.EstadoCuenta)
                    .Select(p => new
                    {
                        p.IdPostulacion,

                        Nombre =
                            p.Graduado.Nombres + " " +
                            p.Graduado.Apellidos,

                        Correo =
                            p.Graduado.Usuario.Correo,

                        p.Graduado.Telefono,

                        p.CvEnviado
                    })
                    .FirstOrDefaultAsync();

            if (contacto is null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Contacto no disponible."
                });
            }

            return Ok(contacto);
        }

        // ==========================================
        // RETIRAR POSTULACIÓN PROPIA
        // ==========================================

        [Authorize(Roles = Graduado)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Retirar(int id)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var postulacion =
                await _context.Postulaciones
                    .Include(p => p.Graduado)
                    .FirstOrDefaultAsync(p =>
                        p.IdPostulacion == id);

            if (postulacion is null)
                return NotFound();

            if (postulacion.Graduado.IdUsuario !=
                idUsuario.Value)
            {
                return Forbid();
            }

            if (postulacion.Estado != Pendiente)
            {
                return Conflict(new
                {
                    mensaje =
                        "Solo se puede retirar una postulacion pendiente."
                });
            }

            _context.Postulaciones.Remove(postulacion);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // FECHA COMPATIBLE CON POSTGRESQL
        // ==========================================

        private static DateTime Ahora() =>
            DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);
    }
}

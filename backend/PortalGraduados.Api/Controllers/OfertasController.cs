
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
    public class OfertasController : ControllerBase
    {
        // ==========================================
        // ROLES Y ESTADOS
        // ==========================================

        private const string Administrador =
            "Administrador de la Escuela";

        private const string Empresa = "Empresa";
        private const string Graduado = "Graduado";

        private const string EnRevision = "En revision";
        private const string Aprobada = "Aprobada";
        private const string Rechazada = "Rechazada";
        private const string Publicada = "Publicada";
        private const string Cerrada = "Cerrada";

        private readonly PortalGraduadosDbContext _context;

        public OfertasController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // PROYECCIÓN DE OFERTAS
        // ==========================================

        private static readonly Expression<Func<Oferta, OfertaDto>>
            Proyeccion = o => new OfertaDto
        {
            IdOferta = o.IdOferta,
            IdEmpresa = o.IdEmpresa,
            NombreEmpresa = o.Empresa.NombreComercial,
            Cargo = o.Cargo,
            Area = o.Area,
            Ubicacion = o.Ubicacion,
            DescripcionCorta = o.DescripcionCorta,
            Descripcion = o.Descripcion,
            Requisitos = o.Requisitos,
            RangoSalarial = o.RangoSalarial,
            Modalidad = o.Modalidad,
            ClasificacionOportunidad = o.ClasificacionOportunidad,
            FechaPublicacion = o.FechaPublicacion,
            FechaCierre = o.FechaCierre,
            Estado = o.Estado
        };

        // ==========================================
        // IDENTIFICAR EMPRESA AUTENTICADA
        // ==========================================

        private async Task<int?> ObtenerEmpresaActual()
        {
            if (!User.IsInRole(Empresa))
                return null;

            if (!int.TryParse(
                User.FindFirst("sub")?.Value,
                out int idUsuario))
            {
                return null;
            }

            return await _context.Empresas
                .Where(e =>
                    e.IdUsuario == idUsuario &&
                    e.EstadoAprobacion &&
                    e.Usuario.EstadoCuenta)
                .Select(e => (int?)e.IdEmpresa)
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // COMPROBAR PROPIEDAD DE UNA OFERTA
        // ==========================================

        private async Task<bool> PuedeGestionarOferta(int id)
        {
            var idEmpresa = await ObtenerEmpresaActual();

            if (!idEmpresa.HasValue)
                return false;

            return await _context.Ofertas.AnyAsync(o =>
                o.IdOferta == id &&
                o.IdEmpresa == idEmpresa.Value);
        }

        // ==========================================
        // CONSULTA SEGÚN ROL
        // ==========================================

        private async Task<IQueryable<Oferta>>
            ObtenerOfertasPermitidas()
        {
            var consulta = _context.Ofertas.AsNoTracking();

            if (User.IsInRole(Administrador))
                return consulta;

            if (User.IsInRole(Empresa))
            {
                var idEmpresa = await ObtenerEmpresaActual();

                if (!idEmpresa.HasValue)
                    return consulta.Where(o => false);

                return consulta.Where(o =>
                    o.IdEmpresa == idEmpresa.Value);
            }

            if (User.IsInRole(Graduado))
            {
                var ahora = Ahora();

                return consulta.Where(o =>
                    o.Estado == Publicada &&
                    o.FechaCierre > ahora &&
                    o.Empresa.EstadoAprobacion &&
                    o.Empresa.Usuario.EstadoCuenta);
            }

            return consulta.Where(o => false);
        }

        // ==========================================
        // RF-09: LISTAR Y FILTRAR OFERTAS
        // ==========================================

        [Authorize(Roles = Administrador + ",Empresa,Graduado")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfertaDto>>> Listar(
            [FromQuery] int? idEmpresa,
            [FromQuery] string? estado,
            [FromQuery] string? area,
            [FromQuery] string? modalidad,
            [FromQuery] string? ubicacion,
            [FromQuery] string? habilidad,
            [FromQuery] string? clasificacion,
            [FromQuery] string? busqueda,
            [FromQuery] bool vigentes = false,
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

            var consulta = await ObtenerOfertasPermitidas();

            if (idEmpresa.HasValue)
            {
                consulta = consulta.Where(o =>
                    o.IdEmpresa == idEmpresa.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var valor = estado.Trim();

                consulta = consulta.Where(o =>
                    o.Estado == valor);
            }

            if (!string.IsNullOrWhiteSpace(modalidad))
            {
                var valor = modalidad.Trim();

                consulta = consulta.Where(o =>
                    o.Modalidad == valor);
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                var valor = area.Trim().ToLower();

                consulta = consulta.Where(o =>
                    o.Area.ToLower() == valor);
            }

            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                var valor = ubicacion.Trim().ToLower();

                consulta = consulta.Where(o =>
                    o.Ubicacion.ToLower().Contains(valor));
            }

            if (!string.IsNullOrWhiteSpace(clasificacion))
            {
                var valor = clasificacion.Trim().ToLower();

                consulta = consulta.Where(o =>
                    o.ClasificacionOportunidad
                        .ToLower().Contains(valor));
            }

            if (!string.IsNullOrWhiteSpace(habilidad))
            {
                var valor = habilidad.Trim().ToLower();

                consulta = consulta.Where(o =>
                    _context.OfertaHabilidades.Any(oh =>
                        oh.IdOferta == o.IdOferta &&
                        oh.Habilidad.NombreHabilidad
                            .ToLower().Contains(valor)));
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var valor = busqueda.Trim().ToLower();

                consulta = consulta.Where(o =>
                    o.Cargo.ToLower().Contains(valor) ||
                    o.Area.ToLower().Contains(valor) ||
                    o.Ubicacion.ToLower().Contains(valor) ||
                    o.DescripcionCorta.ToLower().Contains(valor) ||
                    o.Empresa.NombreComercial
                        .ToLower().Contains(valor));
            }

            if (vigentes)
            {
                var ahora = Ahora();

                consulta = consulta.Where(o =>
                    o.Estado == Publicada &&
                    o.FechaCierre > ahora);
            }

            var total = await consulta.CountAsync();

            Response.Headers["X-Total-Count"] =
                total.ToString();

            var lista = await consulta
                .OrderByDescending(o => o.FechaPublicacion)
                .ThenByDescending(o => o.IdOferta)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // ==========================================
        // CONSULTAR DETALLE DE OFERTA
        // ==========================================

        [Authorize(Roles = Administrador + ",Empresa,Graduado")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OfertaDetalleDto>>
            ObtenerPorId(int id)
        {
            var consulta = await ObtenerOfertasPermitidas();

            var oferta = await consulta
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (oferta is null)
            {
                return NotFound(new
                {
                    mensaje = "Oferta no disponible."
                });
            }

            var habilidades = await ConsultarHabilidades(id);

            return Ok(new OfertaDetalleDto
            {
                Oferta = oferta,
                Habilidades = habilidades
            });
        }

        // ==========================================
        // RF-05: PUBLICAR OFERTA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpPost]
        public async Task<ActionResult<OfertaDto>> Publicar(
            [FromBody] OfertaCrearDto dto)
        {
            var idEmpresa = await ObtenerEmpresaActual();

            if (!idEmpresa.HasValue)
                return Forbid();

            // No confiar en el identificador enviado
            // desde el frontend.

            if (dto.IdEmpresa != idEmpresa.Value)
                return Forbid();

            var ahora = Ahora();

            var cierre = Normalizar(dto.FechaCierre!.Value);

            if (cierre <= ahora)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La fecha de cierre debe ser futura."
                });
            }

            var sinRango =
                string.IsNullOrWhiteSpace(dto.RangoSalarial);

            // RF-21: la primera oferta y las ofertas
            // sin salario requieren revisión.

            var esPrimera = !await _context.Ofertas.AnyAsync(o =>
                o.IdEmpresa == idEmpresa.Value &&
                (
                    o.Estado == Publicada ||
                    o.Estado == Aprobada ||
                    o.Estado == Cerrada
                ));

            var oferta = new Oferta
            {
                IdEmpresa = idEmpresa.Value,
                FechaPublicacion = ahora,
                Estado = sinRango || esPrimera
                    ? EnRevision
                    : Publicada
            };

            Aplicar(oferta, dto, cierre);

            _context.Ofertas.Add(oferta);

            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas
                .AsNoTracking()
                .Where(o => o.IdOferta == oferta.IdOferta)
                .Select(Proyeccion)
                .FirstAsync();

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = oferta.IdOferta },
                respuesta);
        }

        // ==========================================
        // RF-06: MODIFICAR OFERTA PROPIA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OfertaDto>> Actualizar(
            int id,
            [FromBody] OfertaActualizarDto dto)
        {
            if (!await PuedeGestionarOferta(id))
                return Forbid();

            var oferta = await _context.Ofertas.FindAsync(id);

            if (oferta is null)
                return NotFound();

            if (oferta.Estado == Cerrada ||
                oferta.Estado == Rechazada)
            {
                return Conflict(new
                {
                    mensaje =
                        $"No se puede modificar una oferta {oferta.Estado}."
                });
            }

            var cierre =
                Normalizar(dto.FechaCierre!.Value);

            if (cierre <= oferta.FechaPublicacion ||
                cierre <= Ahora())
            {
                return BadRequest(new
                {
                    mensaje = "La fecha de cierre debe ser futura."
                });
            }

            Aplicar(oferta, dto, cierre);

            // Si se elimina el salario de una oferta
            // publicada, vuelve a revisión.

            if (oferta.Estado == Publicada &&
                oferta.RangoSalarial is null)
            {
                oferta.Estado = EnRevision;
            }

            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas
                .AsNoTracking()
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // RF-21: APROBAR O RECHAZAR OFERTA
        // Exclusivo del administrador.
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpPut("{id:int}/revision")]
        public async Task<ActionResult<OfertaDto>> Revisar(
            int id,
            [FromBody] RevisionOfertaDto dto)
        {
            var oferta = await _context.Ofertas
                .Include(o => o.Empresa)
                .ThenInclude(e => e.Usuario)
                .FirstOrDefaultAsync(o =>
                    o.IdOferta == id);

            if (oferta is null)
                return NotFound();

            if (oferta.Estado != EnRevision)
            {
                return Conflict(new
                {
                    mensaje =
                        "Solo se pueden revisar ofertas pendientes."
                });
            }

            var ahora = Ahora();

            if (dto.Aprobada!.Value)
            {
                if (oferta.FechaCierre <= ahora)
                {
                    return Conflict(new
                    {
                        mensaje = "La oferta ya venció."
                    });
                }

                if (!oferta.Empresa.EstadoAprobacion ||
                    !oferta.Empresa.Usuario.EstadoCuenta)
                {
                    return Conflict(new
                    {
                        mensaje = "La empresa no está habilitada."
                    });
                }

                oferta.Estado = Publicada;

                _context.Notificaciones.Add(new Notificacion
                {
                    IdUsuario = oferta.Empresa.IdUsuario,
                    Tipo = "Oferta",
                    Mensaje =
                        $"Tu oferta \"{oferta.Cargo}\" fue aprobada y publicada.",
                    Fecha = ahora,
                    Leida = false
                });
            }
            else
            {
                oferta.Estado = Rechazada;

                _context.Notificaciones.Add(new Notificacion
                {
                    IdUsuario = oferta.Empresa.IdUsuario,
                    Tipo = "Oferta",
                    Mensaje =
                        $"Tu oferta \"{oferta.Cargo}\" fue rechazada. " +
                        $"Motivo: {dto.Motivo!.Trim()}",
                    Fecha = ahora,
                    Leida = false
                });
            }

            // Oferta y notificación se guardan juntas.
            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas
                .AsNoTracking()
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // RF-07: CERRAR OFERTA PROPIA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpPut("{id:int}/cierre")]
        public async Task<ActionResult<OfertaDto>> Cerrar(
            int id)
        {
            if (!await PuedeGestionarOferta(id))
                return Forbid();

            var oferta = await _context.Ofertas.FindAsync(id);

            if (oferta is null)
                return NotFound();

            if (oferta.Estado != Publicada)
            {
                return Conflict(new
                {
                    mensaje =
                        "Solo se puede cerrar una oferta publicada."
                });
            }

            oferta.Estado = Cerrada;

            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas
                .AsNoTracking()
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // ELIMINAR OFERTA PROPIA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!await PuedeGestionarOferta(id))
                return Forbid();

            var oferta = await _context.Ofertas.FindAsync(id);

            if (oferta is null)
                return NotFound();

            if (await _context.Postulaciones.AnyAsync(p =>
                p.IdOferta == id))
            {
                return Conflict(new
                {
                    mensaje =
                        "La oferta tiene postulaciones. Ciérrela en lugar de eliminarla."
                });
            }

            if (await _context.ReportesOferta.AnyAsync(r =>
                r.IdOferta == id))
            {
                return Conflict(new
                {
                    mensaje =
                        "La oferta tiene reportes asociados."
                });
            }

            var habilidades = await _context.OfertaHabilidades
                .Where(oh => oh.IdOferta == id)
                .ToListAsync();

            _context.OfertaHabilidades.RemoveRange(habilidades);

            _context.Ofertas.Remove(oferta);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // CONSULTAR HABILIDADES DE UNA OFERTA
        // ==========================================

        [Authorize(Roles = Administrador + ",Empresa,Graduado")]
        [HttpGet("{id:int}/habilidades")]
        public async Task<ActionResult<IEnumerable<HabilidadDto>>>
            ListarHabilidades(int id)
        {
            var consulta = await ObtenerOfertasPermitidas();

            if (!await consulta.AnyAsync(o =>
                o.IdOferta == id))
            {
                return NotFound();
            }

            return Ok(await ConsultarHabilidades(id));
        }

        // ==========================================
        // ASIGNAR HABILIDAD A OFERTA PROPIA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpPut("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> AsignarHabilidad(
            int id,
            int idHabilidad)
        {
            if (!await PuedeGestionarOferta(id))
                return Forbid();

            if (!await _context.Ofertas.AnyAsync(o =>
                o.IdOferta == id &&
                o.Estado != Cerrada &&
                o.Estado != Rechazada))
            {
                return Conflict(new
                {
                    mensaje = "La oferta no admite modificaciones."
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

            if (await _context.OfertaHabilidades.AnyAsync(oh =>
                oh.IdOferta == id &&
                oh.IdHabilidad == idHabilidad))
            {
                return NoContent();
            }

            _context.OfertaHabilidades.Add(
                new OfertaHabilidad
                {
                    IdOferta = id,
                    IdHabilidad = idHabilidad
                });

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
                // La habilidad ya fue asignada
                // por otra solicitud concurrente.
            }

            return NoContent();
        }

        // ==========================================
        // QUITAR HABILIDAD DE OFERTA PROPIA
        // ==========================================

        [Authorize(Roles = Empresa)]
        [HttpDelete("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> QuitarHabilidad(
            int id,
            int idHabilidad)
        {
            if (!await PuedeGestionarOferta(id))
                return Forbid();

            if (!await _context.Ofertas.AnyAsync(o =>
                o.IdOferta == id &&
                o.Estado != Cerrada &&
                o.Estado != Rechazada))
            {
                return Conflict(new
                {
                    mensaje = "La oferta no admite modificaciones."
                });
            }

            var asignacion = await _context.OfertaHabilidades
                .FindAsync(id, idHabilidad);

            if (asignacion is null)
                return NotFound();

            _context.OfertaHabilidades.Remove(asignacion);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================

        private Task<List<HabilidadDto>> ConsultarHabilidades(
            int idOferta)
        {
            return _context.OfertaHabilidades
                .AsNoTracking()
                .Where(oh => oh.IdOferta == idOferta)
                .OrderBy(oh => oh.Habilidad.NombreHabilidad)
                .Select(oh => new HabilidadDto
                {
                    IdHabilidad = oh.IdHabilidad,
                    NombreHabilidad =
                        oh.Habilidad.NombreHabilidad,
                    Categoria = oh.Habilidad.Categoria
                })
                .ToListAsync();
        }

        private static void Aplicar(
            Oferta oferta,
            OfertaActualizarDto dto,
            DateTime cierre)
        {
            oferta.Cargo = dto.Cargo.Trim();
            oferta.Area = dto.Area.Trim();
            oferta.Ubicacion = dto.Ubicacion.Trim();

            oferta.DescripcionCorta =
                dto.DescripcionCorta.Trim();

            oferta.Descripcion = dto.Descripcion.Trim();
            oferta.Requisitos = dto.Requisitos.Trim();

            oferta.RangoSalarial =
                string.IsNullOrWhiteSpace(dto.RangoSalarial)
                    ? null
                    : dto.RangoSalarial.Trim();

            oferta.Modalidad = dto.Modalidad;

            oferta.ClasificacionOportunidad =
                dto.ClasificacionOportunidad.Trim();

            oferta.FechaCierre = cierre;
        }

        private static DateTime Ahora() =>
            DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);

        private static DateTime Normalizar(DateTime fecha) =>
            fecha.Kind switch
            {
                DateTimeKind.Utc =>
                    DateTime.SpecifyKind(
                        fecha,
                        DateTimeKind.Unspecified),

                DateTimeKind.Local =>
                    DateTime.SpecifyKind(
                        fecha.ToUniversalTime(),
                        DateTimeKind.Unspecified),

                _ => fecha
            };
    }
}

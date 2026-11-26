using System.Linq.Expressions;
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
        private const string EstadoActiva = "Activa";
        private const string EstadoCerrada = "Cerrada";

        private readonly PortalGraduadosDbContext _context;

        public OfertasController(PortalGraduadosDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Oferta, OfertaDto>> Proyeccion = o => new OfertaDto
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

        // GET /api/ofertas?estado=Activa&vigentes=true&area=desarrollo&pagina=1&tamano=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfertaDto>>> Listar(
            [FromQuery] int? idEmpresa,
            [FromQuery] string? estado,
            [FromQuery] string? area,
            [FromQuery] string? modalidad,
            [FromQuery] string? ubicacion,
            [FromQuery] bool vigentes = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "pagina debe ser 1 o mayor y tamano debe estar entre 1 y 100." });

            var consulta = _context.Ofertas.AsNoTracking();

            if (idEmpresa.HasValue)
                consulta = consulta.Where(o => o.IdEmpresa == idEmpresa.Value);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var est = estado.Trim();
                consulta = consulta.Where(o => o.Estado == est);
            }

            if (!string.IsNullOrWhiteSpace(modalidad))
            {
                var mod = modalidad.Trim();
                consulta = consulta.Where(o => o.Modalidad == mod);
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                var a = area.Trim().ToLower();
                consulta = consulta.Where(o => o.Area.ToLower() == a);
            }

            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                var u = ubicacion.Trim().ToLower();
                consulta = consulta.Where(o => o.Ubicacion.ToLower() == u);
            }

            if (vigentes)
            {
                var ahora = Ahora();
                consulta = consulta.Where(o => o.Estado == EstadoActiva && o.FechaCierre > ahora);
            }

            var total = await consulta.CountAsync();
            Response.Headers["X-Total-Count"] = total.ToString();

            var lista = await consulta
                .OrderByDescending(o => o.FechaPublicacion)
                .ThenByDescending(o => o.IdOferta)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/ofertas/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OfertaDetalleDto>> ObtenerPorId(int id)
        {
            var oferta = await _context.Ofertas.AsNoTracking()
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (oferta is null)
                return NotFound(new { mensaje = $"No existe la oferta con id {id}." });

            var habilidades = await ConsultarHabilidades(id);

            return Ok(new OfertaDetalleDto { Oferta = oferta, Habilidades = habilidades });
        }

        // POST /api/ofertas
        [HttpPost]
        public async Task<ActionResult<OfertaDto>> Publicar([FromBody] OfertaCrearDto dto)
        {
            var ahora = Ahora();
            var cierre = Normalizar(dto.FechaCierre!.Value);

            if (cierre <= ahora)
                return BadRequest(new { mensaje = "La fecha de cierre debe ser posterior a la fecha actual." });

            var empresa = await _context.Empresas.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEmpresa == dto.IdEmpresa);

            if (empresa is null)
                return BadRequest(new { mensaje = $"No existe la empresa con id {dto.IdEmpresa}." });

            if (!empresa.EstadoAprobacion)
                return Conflict(new { mensaje = "La empresa aun no ha sido aprobada y no puede publicar ofertas." });

            var oferta = new Oferta
            {
                IdEmpresa = dto.IdEmpresa,
                FechaPublicacion = ahora,
                Estado = EstadoActiva
            };
            Aplicar(oferta, dto, cierre);

            _context.Ofertas.Add(oferta);
            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas.AsNoTracking()
                .Where(o => o.IdOferta == oferta.IdOferta)
                .Select(Proyeccion)
                .FirstAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = oferta.IdOferta }, respuesta);
        }

        // PUT /api/ofertas/1
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OfertaDto>> Actualizar(int id, [FromBody] OfertaActualizarDto dto)
        {
            var oferta = await _context.Ofertas.FindAsync(id);

            if (oferta is null)
                return NotFound(new { mensaje = $"No existe la oferta con id {id}." });

            var cierre = Normalizar(dto.FechaCierre!.Value);

            if (cierre <= oferta.FechaPublicacion)
                return BadRequest(new { mensaje = "La fecha de cierre debe ser posterior a la fecha de publicacion." });

            if (oferta.Estado == EstadoActiva && cierre <= Ahora())
                return BadRequest(new { mensaje = "Una oferta activa necesita una fecha de cierre futura. Cierrela primero si desea terminarla." });

            Aplicar(oferta, dto, cierre);
            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas.AsNoTracking()
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // PUT /api/ofertas/1/estado
        [HttpPut("{id:int}/estado")]
        public async Task<ActionResult<OfertaDto>> CambiarEstado(int id, [FromBody] CambioEstadoOfertaDto dto)
        {
            var oferta = await _context.Ofertas.FindAsync(id);

            if (oferta is null)
                return NotFound(new { mensaje = $"No existe la oferta con id {id}." });

            if (dto.Estado == EstadoActiva)
            {
                if (oferta.FechaCierre <= Ahora())
                    return Conflict(new { mensaje = "La fecha de cierre ya paso. Actualice la oferta con una fecha futura antes de reabrirla." });

                var empresaAprobada = await _context.Empresas
                    .AnyAsync(e => e.IdEmpresa == oferta.IdEmpresa && e.EstadoAprobacion);

                if (!empresaAprobada)
                    return Conflict(new { mensaje = "La empresa no esta aprobada y no puede tener ofertas activas." });
            }

            oferta.Estado = dto.Estado;
            await _context.SaveChangesAsync();

            var respuesta = await _context.Ofertas.AsNoTracking()
                .Where(o => o.IdOferta == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // DELETE /api/ofertas/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var oferta = await _context.Ofertas.FindAsync(id);

            if (oferta is null)
                return NotFound(new { mensaje = $"No existe la oferta con id {id}." });

            if (await _context.Postulaciones.AnyAsync(p => p.IdOferta == id))
                return Conflict(new { mensaje = "La oferta tiene postulaciones. Cierrela en lugar de eliminarla." });

            if (await _context.ReportesOferta.AnyAsync(r => r.IdOferta == id))
                return Conflict(new { mensaje = "La oferta tiene reportes asociados y no se puede eliminar." });

            // Las FK son Restrict: primero se quitan las habilidades requeridas.
            // EF ejecuta todo en una sola transaccion (si algo falla, no se borra nada).
            var habilidades = await _context.OfertaHabilidades
                .Where(oh => oh.IdOferta == id)
                .ToListAsync();

            _context.OfertaHabilidades.RemoveRange(habilidades);
            _context.Ofertas.Remove(oferta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/ofertas/1/habilidades
        [HttpGet("{id:int}/habilidades")]
        public async Task<ActionResult<IEnumerable<HabilidadDto>>> ListarHabilidades(int id)
        {
            if (!await _context.Ofertas.AnyAsync(o => o.IdOferta == id))
                return NotFound(new { mensaje = $"No existe la oferta con id {id}." });

            return Ok(await ConsultarHabilidades(id));
        }

        // PUT /api/ofertas/1/habilidades/3  (idempotente)
        [HttpPut("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> AsignarHabilidad(int id, int idHabilidad)
        {
            if (!await _context.Ofertas.AnyAsync(o => o.IdOferta == id))
                return NotFound(new { mensaje = $"No existe la oferta con id {id}." });

            if (!await _context.Habilidades.AnyAsync(h => h.IdHabilidad == idHabilidad))
                return NotFound(new { mensaje = $"No existe la habilidad con id {idHabilidad}." });

            if (await _context.OfertaHabilidades.AnyAsync(oh => oh.IdOferta == id && oh.IdHabilidad == idHabilidad))
                return NoContent();

            _context.OfertaHabilidades.Add(new OfertaHabilidad
            {
                IdOferta = id,
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

        // DELETE /api/ofertas/1/habilidades/3
        [HttpDelete("{id:int}/habilidades/{idHabilidad:int}")]
        public async Task<IActionResult> QuitarHabilidad(int id, int idHabilidad)
        {
            var asignacion = await _context.OfertaHabilidades.FindAsync(id, idHabilidad);

            if (asignacion is null)
                return NotFound(new { mensaje = "La oferta no tiene asignada esa habilidad." });

            _context.OfertaHabilidades.Remove(asignacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<List<HabilidadDto>> ConsultarHabilidades(int idOferta) =>
            _context.OfertaHabilidades.AsNoTracking()
                .Where(oh => oh.IdOferta == idOferta)
                .OrderBy(oh => oh.Habilidad.NombreHabilidad)
                .Select(oh => new HabilidadDto
                {
                    IdHabilidad = oh.IdHabilidad,
                    NombreHabilidad = oh.Habilidad.NombreHabilidad,
                    Categoria = oh.Habilidad.Categoria
                })
                .ToListAsync();

        private static void Aplicar(Oferta oferta, OfertaActualizarDto dto, DateTime cierre)
        {
            oferta.Cargo = dto.Cargo.Trim();
            oferta.Area = dto.Area.Trim();
            oferta.Ubicacion = dto.Ubicacion.Trim();
            oferta.DescripcionCorta = dto.DescripcionCorta.Trim();
            oferta.Descripcion = dto.Descripcion.Trim();
            oferta.Requisitos = dto.Requisitos.Trim();
            oferta.RangoSalarial = string.IsNullOrWhiteSpace(dto.RangoSalarial) ? null : dto.RangoSalarial.Trim();
            oferta.Modalidad = dto.Modalidad;
            oferta.ClasificacionOportunidad = dto.ClasificacionOportunidad.Trim();
            oferta.FechaCierre = cierre;
        }

        // Las columnas son "timestamp without time zone": se guarda en UTC y sin Kind
        private static DateTime Ahora() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // Una fecha sin zona horaria se toma como UTC; una con zona se convierte a UTC
        private static DateTime Normalizar(DateTime fecha) => fecha.Kind switch
        {
            DateTimeKind.Utc => DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified),
            DateTimeKind.Local => DateTime.SpecifyKind(fecha.ToUniversalTime(), DateTimeKind.Unspecified),
            _ => fecha
        };
    }
}
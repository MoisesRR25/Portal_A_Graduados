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
    public class PostulacionesController : ControllerBase
    {
        
        private const string Pendiente = "Pendiente";
        private const string EnRevision = "En revision";
        private const string Aceptada = "Aceptada";
        private const string Rechazada = "Rechazada";

        private const string OfertaPublicada = "Publicada";

        private static readonly Dictionary<string, string[]> Transiciones = new()
        {
            [Pendiente] = new[] { EnRevision, Rechazada },
            [EnRevision] = new[] { Aceptada, Rechazada },
            [Aceptada] = Array.Empty<string>(),
            [Rechazada] = Array.Empty<string>()
        };

        private readonly PortalGraduadosDbContext _context;

        public PostulacionesController(PortalGraduadosDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Postulacion, PostulacionDto>> Proyeccion = p => new PostulacionDto
        {
            IdPostulacion = p.IdPostulacion,
            IdGraduado = p.IdGraduado,
            NombreGraduado = p.Graduado.Nombres + " " + p.Graduado.Apellidos,
            IdOferta = p.IdOferta,
            CargoOferta = p.Oferta.Cargo,
            IdEmpresa = p.Oferta.IdEmpresa,
            NombreEmpresa = p.Oferta.Empresa.NombreComercial,
            FechaPostulacion = p.FechaPostulacion,
            CvEnviado = p.CvEnviado,
            Estado = p.Estado
        };

        // GET /api/postulaciones?idGraduado=1&idOferta=1&idEmpresa=1&estado=Pendiente&pagina=1&tamano=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostulacionDto>>> Listar(
            [FromQuery] int? idGraduado,
            [FromQuery] int? idOferta,
            [FromQuery] int? idEmpresa,
            [FromQuery] string? estado,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            if (pagina < 1 || tamano < 1 || tamano > 100)
                return BadRequest(new { mensaje = "pagina debe ser 1 o mayor y tamano debe estar entre 1 y 100." });

            var consulta = _context.Postulaciones.AsNoTracking();

            if (idGraduado.HasValue)
                consulta = consulta.Where(p => p.IdGraduado == idGraduado.Value);

            if (idOferta.HasValue)
                consulta = consulta.Where(p => p.IdOferta == idOferta.Value);

            if (idEmpresa.HasValue)
                consulta = consulta.Where(p => p.Oferta.IdEmpresa == idEmpresa.Value);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var est = estado.Trim();
                consulta = consulta.Where(p => p.Estado == est);
            }

            var total = await consulta.CountAsync();
            Response.Headers["X-Total-Count"] = total.ToString();

            var lista = await consulta
                .OrderByDescending(p => p.FechaPostulacion)
                .ThenByDescending(p => p.IdPostulacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/postulaciones/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PostulacionDto>> ObtenerPorId(int id)
        {
            var postulacion = await _context.Postulaciones.AsNoTracking()
                .Where(p => p.IdPostulacion == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (postulacion is null)
                return NotFound(new { mensaje = $"No existe la postulacion con id {id}." });

            return Ok(postulacion);
        }

        // POST /api/postulaciones
        [HttpPost]
        public async Task<ActionResult<PostulacionDto>> Postular([FromBody] PostulacionCrearDto dto)
        {
            var graduado = await _context.Graduados.AsNoTracking()
                .FirstOrDefaultAsync(g => g.IdGraduado == dto.IdGraduado);

            if (graduado is null)
                return BadRequest(new { mensaje = $"No existe el graduado con id {dto.IdGraduado}." });

            var oferta = await _context.Ofertas.AsNoTracking()
                .FirstOrDefaultAsync(o => o.IdOferta == dto.IdOferta);

            if (oferta is null)
                return BadRequest(new { mensaje = $"No existe la oferta con id {dto.IdOferta}." });

           if (!graduado.Verificado || !graduado.Consentimiento)
           return Conflict(new { mensaje = "El perfil debe estar verificado y con el consentimiento aceptado para poder postular." });

            var ahora = Ahora();

            if (oferta.Estado != OfertaPublicada || oferta.FechaCierre <= ahora)
            return Conflict(new { mensaje = "La oferta no esta publicada o ya vencio, y no acepta postulaciones." });

            var cv = string.IsNullOrWhiteSpace(dto.CvEnviado) ? graduado.CvAdjunto : dto.CvEnviado.Trim();

            if (string.IsNullOrWhiteSpace(cv))
                return BadRequest(new { mensaje = "Debe enviar un CV: indiquelo en la solicitud o cargue uno en el perfil del graduado." });

            if (await _context.Postulaciones.AnyAsync(p => p.IdGraduado == dto.IdGraduado && p.IdOferta == dto.IdOferta))
                return Conflict(new { mensaje = "El graduado ya se postulo a esta oferta." });

            var postulacion = new Postulacion
            {
                IdGraduado = dto.IdGraduado,
                IdOferta = dto.IdOferta,
                FechaPostulacion = ahora,
                CvEnviado = cv,
                Estado = Pendiente
            };

            _context.Postulaciones.Add(postulacion);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Dos peticiones simultaneas: el indice unico evita el duplicado
                return Conflict(new { mensaje = "El graduado ya se postulo a esta oferta." });
            }

            var respuesta = await _context.Postulaciones.AsNoTracking()
                .Where(p => p.IdPostulacion == postulacion.IdPostulacion)
                .Select(Proyeccion)
                .FirstAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = postulacion.IdPostulacion }, respuesta);
        }

        // PUT /api/postulaciones/1/estado
        [HttpPut("{id:int}/estado")]
        public async Task<ActionResult<PostulacionDto>> CambiarEstado(int id, [FromBody] CambioEstadoPostulacionDto dto)
        {
            var postulacion = await _context.Postulaciones.FindAsync(id);

            if (postulacion is null)
                return NotFound(new { mensaje = $"No existe la postulacion con id {id}." });

            if (!Transiciones.TryGetValue(postulacion.Estado, out var permitidos) || !permitidos.Contains(dto.Estado))
                return Conflict(new { mensaje = $"No se puede pasar una postulacion de '{postulacion.Estado}' a '{dto.Estado}'." });

            postulacion.Estado = dto.Estado;
            await _context.SaveChangesAsync();

            var respuesta = await _context.Postulaciones.AsNoTracking()
                .Where(p => p.IdPostulacion == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // DELETE /api/postulaciones/1  (retirar la postulacion)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Retirar(int id)
        {
            var postulacion = await _context.Postulaciones.FindAsync(id);

            if (postulacion is null)
                return NotFound(new { mensaje = $"No existe la postulacion con id {id}." });

            if (postulacion.Estado != Pendiente)
                return Conflict(new { mensaje = "Solo se puede retirar una postulacion que sigue pendiente." });

            _context.Postulaciones.Remove(postulacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // La columna es "timestamp without time zone": se guarda en UTC y sin Kind
        private static DateTime Ahora() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }
}
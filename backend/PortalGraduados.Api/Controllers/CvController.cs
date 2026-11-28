
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/cv")]
    public class CvController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        private const long MaximoBytes =
            5L * 1024 * 1024;

        public CvController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

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
        // SUBIR CV DEL GRADUADO
        // ==========================================

        [Authorize(Roles = "Graduado")]
        [HttpPost]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(
            MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        public async Task<IActionResult> Subir(
            [FromForm] IFormFile archivo,
            CancellationToken cancellationToken)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var graduado = await _context.Graduados
                .FirstOrDefaultAsync(g =>
                    g.IdUsuario == idUsuario.Value,
                    cancellationToken);

            if (graduado is null)
                return Forbid();

            // VALIDAR TAMAÑO

            if (archivo is null || archivo.Length < 5)
            {
                return BadRequest(new
                {
                    mensaje = "Selecciona un archivo PDF valido."
                });
            }

            if (archivo.Length > MaximoBytes)
            {
                return BadRequest(new
                {
                    mensaje = "El CV no puede superar los 5 MB."
                });
            }

            // VALIDAR EXTENSIÓN

            if (!string.Equals(
                Path.GetExtension(archivo.FileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje = "Solo se permiten archivos PDF."
                });
            }

            // COMPROBAR CABECERA DEL DOCUMENTO

            await using (var lectura =
                archivo.OpenReadStream())
            {
                var cabecera = new byte[5];

                await lectura.ReadExactlyAsync(
                    cabecera.AsMemory(),
                    cancellationToken);

                if (Encoding.ASCII.GetString(cabecera)
                    != "%PDF-")
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El archivo no tiene una cabecera PDF valida."
                    });
                }
            }

            // GENERAR NOMBRE PRIVADO

            Directory.CreateDirectory(
                CvArchivos.Carpeta);

            var referencia =
                $"g{graduado.IdGraduado}-{Guid.NewGuid():N}.pdf";

            var ruta = Path.Combine(
                CvArchivos.Carpeta,
                referencia);

            // GUARDAR ARCHIVO

            await using (var destino = new FileStream(
                ruta,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                await archivo.CopyToAsync(
                    destino,
                    cancellationToken);
            }

            // GUARDAR REFERENCIA EN POSTGRESQL

            graduado.CvAdjunto = referencia;

            graduado.FechaUltimaActualizacion =
                DateTime.SpecifyKind(
                    DateTime.UtcNow,
                    DateTimeKind.Unspecified);

            await _context.SaveChangesAsync(
                cancellationToken);

            return Ok(new
            {
                mensaje = "CV almacenado correctamente.",
                cvAdjunto = referencia,
                tamano = archivo.Length
            });
        }

        // ==========================================
        // DESCARGAR CV PROPIO
        // ==========================================

        [Authorize(Roles = "Graduado")]
        [HttpGet("propio")]
        public async Task<IActionResult> DescargarPropio()
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var graduado = await _context.Graduados
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.IdUsuario == idUsuario.Value);

            if (graduado is null)
                return NotFound();

            if (!CvArchivos.ObtenerRuta(
                graduado.IdGraduado,
                graduado.CvAdjunto,
                out var ruta))
            {
                return NotFound(new
                {
                    mensaje = "No existe un CV almacenado."
                });
            }

            Response.Headers["Cache-Control"] =
                "no-store";

            Response.Headers["X-Content-Type-Options"] =
                "nosniff";

            return PhysicalFile(
                ruta,
                "application/pdf",
                "Mi_curriculum.pdf");
        }

        // ==========================================
        // DESCARGAR CV DE UNA POSTULACIÓN
        // ==========================================

        [Authorize(Roles =
            "Administrador de la Escuela,Graduado,Empresa")]
        [HttpGet("postulaciones/{id:int}")]
        public async Task<IActionResult>
            DescargarPostulacion(int id)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (!idUsuario.HasValue)
                return Unauthorized();

            var consulta = _context.Postulaciones
                .AsNoTracking()
                .Where(p =>
                    p.IdPostulacion == id);

            // VALIDAR PROPIEDAD DE LA POSTULACIÓN

            if (User.IsInRole("Graduado"))
            {
                consulta = consulta.Where(p =>
                    p.Graduado.IdUsuario ==
                        idUsuario.Value);
            }
            else if (User.IsInRole("Empresa"))
            {
                consulta = consulta.Where(p =>
                    p.Oferta.Empresa.IdUsuario ==
                        idUsuario.Value &&
                    p.Graduado.Consentimiento &&
                    p.Graduado.Verificado &&
                    p.Graduado.Usuario.EstadoCuenta);
            }
            else if (!User.IsInRole(
                "Administrador de la Escuela"))
            {
                return Forbid();
            }

            var postulacion = await consulta
                .Select(p => new
                {
                    p.IdGraduado,
                    p.CvEnviado
                })
                .FirstOrDefaultAsync();

            if (postulacion is null)
                return NotFound();

            if (!CvArchivos.ObtenerRuta(
                postulacion.IdGraduado,
                postulacion.CvEnviado,
                out var ruta))
            {
                return NotFound(new
                {
                    mensaje =
                        "El documento de esta postulacion no esta disponible."
                });
            }

            Response.Headers["Cache-Control"] =
                "no-store";

            Response.Headers["X-Content-Type-Options"] =
                "nosniff";

            return PhysicalFile(
                ruta,
                "application/pdf",
                $"CV_Postulacion_{id}.pdf");
        }
    }

    // ==========================================
    // VALIDACIÓN DE ARCHIVOS PRIVADOS
    // ==========================================

    internal static class CvArchivos
    {
        public static string Carpeta =>
            Path.Combine(
                AppContext.BaseDirectory,
                "cv");

        public static bool ObtenerRuta(
            int idGraduado,
            string? referencia,
            out string ruta)
        {
            ruta = string.Empty;

            if (string.IsNullOrWhiteSpace(referencia))
                return false;

            // Solo aceptar nombres generados por la API
            // y pertenecientes al graduado indicado.

            var patron =
                $@"\Ag{idGraduado}-[a-f0-9]{{32}}\.pdf\z";

            if (!Regex.IsMatch(referencia, patron))
                return false;

            ruta = Path.Combine(
                Carpeta,
                referencia);

            return System.IO.File.Exists(ruta);
        }
    }
}


using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [Authorize(Roles = "Administrador de la Escuela")]
    [ApiController]
    [Route("api/carga-masiva")]
    public class CargaMasivaController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        private readonly IPasswordHasher<Usuario> _hasher;

        private const int MaximoRegistros = 500;

        private static readonly Regex FormatoCarnet =
            new(@"^[A-Z]{2}\d{5}$");

        public CargaMasivaController(
            PortalGraduadosDbContext context,
            IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // ==========================================
        // RF-13: IMPORTAR GRADUADOS
        // ==========================================

        [HttpPost]
        public async Task<ActionResult<CargaMasivaRespuestaDto>>
            Importar([FromBody] CargaMasivaRequestDto dto)
        {
            if (!int.TryParse(
                User.FindFirst("sub")?.Value,
                out int idAdministrador))
            {
                return Unauthorized();
            }

            if (dto.Registros is null ||
                dto.Registros.Count == 0)
            {
                return BadRequest(new
                {
                    mensaje = "No existen registros para importar."
                });
            }

            if (dto.Registros.Count > MaximoRegistros)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"Se permiten hasta {MaximoRegistros} registros por carga."
                });
            }

            var rolGraduado = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.NombreRol == "Graduado");

            if (rolGraduado is null)
            {
                return StatusCode(500, new
                {
                    mensaje = "No existe el rol Graduado."
                });
            }

            var ahora = Ahora();

            // ==========================================
            // CONSULTAR DUPLICADOS EN POSTGRESQL
            // ==========================================

            var correosSolicitados = dto.Registros
                .Where(r => r is not null)
                .Select(r => NormalizarCorreo(r!.Correo))
                .Where(c => c.Length > 0)
                .Distinct()
                .ToList();

            var carnetsSolicitados = dto.Registros
                .Where(r => r is not null)
                .Select(r => NormalizarCarnet(r!.Carnet))
                .Where(c => c.Length > 0)
                .Distinct()
                .ToList();

            var correosExistentes = (
                await _context.Usuarios
                    .AsNoTracking()
                    .Where(u =>
                        correosSolicitados.Contains(
                            u.Correo.ToLower()))
                    .Select(u => u.Correo)
                    .ToListAsync()
            ).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var carnetsExistentes = (
                await _context.Graduados
                    .AsNoTracking()
                    .Where(g =>
                        g.Carnet != null &&
                        carnetsSolicitados.Contains(g.Carnet))
                    .Select(g => g.Carnet!)
                    .ToListAsync()
            ).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var correosArchivo =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var carnetsArchivo =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var resultados =
                new List<ResultadoRegistroCargaDto>();

            var registrosValidos =
                new List<(
                    Graduado Perfil,
                    ResultadoRegistroCargaDto Resultado
                )>();

            // ==========================================
            // VALIDAR CADA FILA
            // ==========================================

            for (int i = 0; i < dto.Registros.Count; i++)
            {
                var registro = dto.Registros[i]
                    ?? new RegistroCargaDto();

                var resultado =
                    new ResultadoRegistroCargaDto
                    {
                        Fila = registro.Fila >= 2
                            ? registro.Fila
                            : i + 2,

                        Nombre = Limpiar(registro.Nombre),

                        Correo =
                            NormalizarCorreo(registro.Correo),

                        Carnet =
                            NormalizarCarnet(registro.Carnet),

                        Anio = Limpiar(registro.Anio),

                        EstadoLaboral =
                            Limpiar(registro.EstadoLaboral)
                    };

                resultados.Add(resultado);

                // VALIDAR NOMBRE COMPLETO

                var partesNombre = Regex.Split(
                    resultado.Nombre,
                    @"\s+");

                var nombres =
                    partesNombre.FirstOrDefault()
                    ?? string.Empty;

                var apellidos = string.Join(
                    " ",
                    partesNombre.Skip(1));

                if (nombres.Length < 2 ||
                    apellidos.Length < 2 ||
                    nombres.Length > 100 ||
                    apellidos.Length > 100)
                {
                    resultado.Errores.Add(
                        "Nombre completo invalido.");
                }

                // VALIDAR CORREO

                if (string.IsNullOrWhiteSpace(
                        resultado.Correo) ||
                    resultado.Correo.Length > 150 ||
                    !new EmailAddressAttribute()
                        .IsValid(resultado.Correo))
                {
                    resultado.Errores.Add(
                        "Correo electronico invalido.");
                }

                if (resultado.Correo.Length > 0)
                {
                    if (!correosArchivo.Add(
                        resultado.Correo))
                    {
                        resultado.Errores.Add(
                            "Correo duplicado dentro del archivo.");
                    }

                    if (correosExistentes.Contains(
                        resultado.Correo))
                    {
                        resultado.Errores.Add(
                            "El correo ya esta registrado en el portal.");
                    }
                }

                // VALIDAR CARNET

                if (!FormatoCarnet.IsMatch(
                    resultado.Carnet))
                {
                    resultado.Errores.Add(
                        "Carnet invalido. Debe tener dos letras y cinco numeros.");
                }

                if (resultado.Carnet.Length > 0)
                {
                    if (!carnetsArchivo.Add(
                        resultado.Carnet))
                    {
                        resultado.Errores.Add(
                            "Carnet duplicado dentro del archivo.");
                    }

                    if (carnetsExistentes.Contains(
                        resultado.Carnet))
                    {
                        resultado.Errores.Add(
                            "El carnet ya existe en PostgreSQL.");
                    }
                }

                // VALIDAR AÑO DE GRADUACIÓN

                if (!Regex.IsMatch(
                        resultado.Anio,
                        @"^\d{4}$") ||
                    !int.TryParse(
                        resultado.Anio,
                        out _) ||
                    int.Parse(resultado.Anio) < 1990 ||
                    int.Parse(resultado.Anio) >
                        DateTime.UtcNow.Year)
                {
                    resultado.Errores.Add(
                        "Anio de graduacion invalido.");
                }

                // VALIDAR ESTADO LABORAL

                if (string.IsNullOrWhiteSpace(
                        resultado.EstadoLaboral) ||
                    resultado.EstadoLaboral.Length > 30)
                {
                    resultado.Errores.Add(
                        "El estado laboral es obligatorio y admite hasta 30 caracteres.");
                }

                // Los registros inválidos no se guardan.

                if (resultado.Errores.Count > 0)
                    continue;

                // ==========================================
                // PREPARAR CUENTA PENDIENTE
                // ==========================================

                var usuario = new Usuario
                {
                    IdRol = rolGraduado.IdRol,

                    Correo = resultado.Correo,

                    EstadoCuenta = false
                };

                // Contraseña aleatoria que no se entrega.
                // El usuario deberá establecer su contraseña
                // mediante un proceso seguro de activación.

                var claveTemporal = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(32));

                usuario.ContrasenaHash =
                    _hasher.HashPassword(
                        usuario,
                        claveTemporal);

                // ==========================================
                // PREPARAR PERFIL DEL GRADUADO
                // ==========================================

                var graduado = new Graduado
                {
                    Usuario = usuario,

                    Nombres = nombres,

                    Apellidos = apellidos,

                    Carnet = resultado.Carnet,

                    AnioGraduacion =
                        int.Parse(resultado.Anio),

                    EstadoAcademico = "Graduado",

                    EstadoLaboral =
                        resultado.EstadoLaboral,

                    // Datos todavía no proporcionados.

                    Telefono = string.Empty,

                    AreaTrabajo = string.Empty,

                    Ubicacion = string.Empty,

                    CvAdjunto = null,

                    PortafolioEnlace = null,

                    // RN-06: perfil pendiente de activación.

                    Verificado = false,

                    Consentimiento = false,

                    FechaUltimaActualizacion = ahora
                };

                registrosValidos.Add(
                    (graduado, resultado));
            }

            // ==========================================
            // PREPARAR HISTORIAL DE CARGA
            // ==========================================

            var carga = new CargaPerfiles
            {
                IdUsuario = idAdministrador,

                FechaCarga = ahora,

                TotalRegistros = resultados.Count,

                TotalCargados = registrosValidos.Count,

                TotalRechazados =
                    resultados.Count -
                    registrosValidos.Count
            };

            // ==========================================
            // GUARDAR MEDIANTE TRANSACCIÓN
            // ==========================================

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var registro in registrosValidos)
                {
                    // EF Core crea el usuario relacionado
                    // junto con el perfil del graduado.

                    _context.Graduados.Add(registro.Perfil);
                }

                _context.CargasPerfiles.Add(carga);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                foreach (var registro in registrosValidos)
                {
                    registro.Resultado.IdGraduado =
                        registro.Perfil.IdGraduado;
                }

                return StatusCode(
                    StatusCodes.Status201Created,
                    new CargaMasivaRespuestaDto
                    {
                        IdCarga = carga.IdCarga,

                        FechaCarga = carga.FechaCarga,

                        TotalRegistros =
                            carga.TotalRegistros,

                        TotalCargados =
                            carga.TotalCargados,

                        TotalRechazados =
                            carga.TotalRechazados,

                        Resultados = resultados
                    });
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
                    mensaje =
                        "Se detecto un registro duplicado durante la importacion. No se guardo la carga. Vuelve a intentarlo."
                });
            }
        }

        // ==========================================
        // CONSULTAR HISTORIAL DE CARGAS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Historial(
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
                _context.CargasPerfiles.AsNoTracking();

            Response.Headers["X-Total-Count"] =
                (await consulta.CountAsync()).ToString();

            var historial = await consulta
                .OrderByDescending(c => c.FechaCarga)
                .ThenByDescending(c => c.IdCarga)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(c => new
                {
                    c.IdCarga,

                    c.FechaCarga,

                    c.IdUsuario,

                    c.TotalRegistros,

                    c.TotalCargados,

                    c.TotalRechazados
                })
                .ToListAsync();

            return Ok(historial);
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================

        private static string Limpiar(string? texto)
        {
            return texto?.Trim() ?? string.Empty;
        }

        private static string NormalizarCorreo(string? correo)
        {
            return Limpiar(correo).ToLowerInvariant();
        }

        private static string NormalizarCarnet(string? carnet)
        {
            return Limpiar(carnet).ToUpperInvariant();
        }

        private static DateTime Ahora()
        {
            return DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);
        }
    }
}

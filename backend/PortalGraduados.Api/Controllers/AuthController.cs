
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.DTOs;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        private readonly IPasswordHasher<Usuario> _hasher;

        private readonly IConfiguration _configuration;

        public AuthController(
            PortalGraduadosDbContext context,
            IPasswordHasher<Usuario> hasher,
            IConfiguration configuration)
        {
            _context = context;
            _hasher = hasher;
            _configuration = configuration;
        }

        // ==========================================
        // POST /api/auth/login
        // ==========================================

        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost("login")]
        public async Task<ActionResult<LoginRespuestaDto>> Login(
            [FromBody] LoginDto dto)
        {
            var correo = dto.Correo
                .Trim()
                .ToLowerInvariant();

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(
                    u => u.Correo == correo
                );

            if (usuario is null)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            PasswordVerificationResult resultado;

            try
            {
                resultado = _hasher.VerifyHashedPassword(
                    usuario,
                    usuario.ContrasenaHash,
                    dto.Contrasena
                );
            }
            catch (FormatException)
            {
                resultado = PasswordVerificationResult.Failed;
            }

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            // ==========================================
            // VALIDAR CUENTA
            // ==========================================

            if (!usuario.EstadoCuenta)
            {
                return StatusCode(403, new
                {
                    mensaje = "La cuenta no está activa."
                });
            }

            var nombreRol = usuario.Rol.NombreRol;

            if (nombreRol == "Graduado")
            {
                var graduado = await _context.Graduados
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        g => g.IdUsuario == usuario.IdUsuario
                    );

                if (graduado is null ||
                    !graduado.Verificado ||
                    !graduado.Consentimiento)
                {
                    return StatusCode(403, new
                    {
                        mensaje =
                            "El perfil está pendiente de activación."
                    });
                }
            }
            else if (nombreRol == "Empresa")
            {
                var empresa = await _context.Empresas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        e => e.IdUsuario == usuario.IdUsuario
                    );

                if (empresa is null ||
                    !empresa.EstadoAprobacion)
                {
                    return StatusCode(403, new
                    {
                        mensaje =
                            "La empresa está pendiente de aprobación."
                    });
                }
            }
            else if (nombreRol != "Administrador de la Escuela")
            {
                return StatusCode(403, new
                {
                    mensaje = "Rol no autorizado."
                });
            }

            // Actualizar el hash si utiliza un formato antiguo.

            if (resultado ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.ContrasenaHash =
                    _hasher.HashPassword(
                        usuario,
                        dto.Contrasena
                    );

                await _context.SaveChangesAsync();
            }

            // ==========================================
            // GENERAR TOKEN
            // ==========================================

            var expiracion =
                DateTime.UtcNow.AddMinutes(60);

            var token = GenerarToken(
                usuario,
                expiracion
            );

            var datosUsuario =
                await ObtenerDatosUsuario(usuario);

            return Ok(new LoginRespuestaDto
            {
                Token = token,
                TipoToken = "Bearer",
                Expiracion = expiracion,
                Usuario = datosUsuario
            });
        }

        // ==========================================
        // GET /api/auth/me
        // ==========================================

        [Authorize]
        [HttpGet("me")]
        public async Task<
            ActionResult<UsuarioAutenticadoDto>
        > ObtenerUsuarioActual()
        {
            var identificador =
                User.FindFirst("sub")?.Value;

            if (!int.TryParse(
                identificador,
                out int idUsuario))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario
                );

            if (usuario is null ||
                !usuario.EstadoCuenta)
            {
                return Unauthorized();
            }

            return Ok(
                await ObtenerDatosUsuario(usuario)
            );
        }

        // ==========================================
        // GENERAR JWT
        // ==========================================

        private string GenerarToken(
            Usuario usuario,
            DateTime expiracion)
        {
            var clave = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Clave JWT no configurada."
                );

            var issuer = _configuration["Jwt:Issuer"]
                ?? "PortalGraduados.Api";

            var audience = _configuration["Jwt:Audience"]
                ?? "PortalGraduados.Angular";

            var claims = new List<Claim>
            {
                new Claim(
                    "sub",
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    "email",
                    usuario.Correo
                ),

                new Claim(
                    "role",
                    usuario.Rol.NombreRol
                ),

                new Claim(
                    "jti",
                    Guid.NewGuid().ToString()
                )
            };

            var llave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave)
            );

            var credenciales = new SigningCredentials(
                llave,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiracion,
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        // ==========================================
        // DATOS DEL USUARIO AUTENTICADO
        // ==========================================

        private async Task<UsuarioAutenticadoDto>
            ObtenerDatosUsuario(Usuario usuario)
        {
            int? idGraduado = null;

            int? idEmpresa = null;

            if (usuario.Rol.NombreRol == "Graduado")
            {
                idGraduado = await _context.Graduados
                    .Where(g =>
                        g.IdUsuario == usuario.IdUsuario
                    )
                    .Select(g => (int?)g.IdGraduado)
                    .FirstOrDefaultAsync();
            }

            if (usuario.Rol.NombreRol == "Empresa")
            {
                idEmpresa = await _context.Empresas
                    .Where(e =>
                        e.IdUsuario == usuario.IdUsuario
                    )
                    .Select(e => (int?)e.IdEmpresa)
                    .FirstOrDefaultAsync();
            }

            return new UsuarioAutenticadoDto
            {
                IdUsuario = usuario.IdUsuario,
                Correo = usuario.Correo,
                Rol = usuario.Rol.NombreRol,
                IdGraduado = idGraduado,
                IdEmpresa = idEmpresa
            };
        }
    }
}

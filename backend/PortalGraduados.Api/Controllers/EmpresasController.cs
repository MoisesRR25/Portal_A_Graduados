
using System.Linq.Expressions;
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
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private const string NombreRolEmpresa = "Empresa";

        private const string Administrador =
            "Administrador de la Escuela";

        private readonly PortalGraduadosDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public EmpresasController(
            PortalGraduadosDbContext context,
            IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // ==========================================
        // PROYECCIÓN DE DATOS
        // ==========================================

        private static readonly Expression<Func<Empresa, EmpresaDto>>
            Proyeccion = e => new EmpresaDto
        {
            IdEmpresa = e.IdEmpresa,
            IdUsuario = e.IdUsuario,
            Correo = e.Usuario.Correo,
            NombreComercial = e.NombreComercial,
            RazonSocial = e.RazonSocial,
            TelefonoContacto = e.TelefonoContacto,
            EstadoAprobacion = e.EstadoAprobacion
        };

        // ==========================================
        // COMPROBAR PROPIEDAD DE LA EMPRESA
        // ==========================================

        private async Task<bool> PuedeAccederEmpresa(int id)
        {
            if (User.IsInRole(Administrador))
                return true;

            if (!User.IsInRole(NombreRolEmpresa))
                return false;

            if (!int.TryParse(
                User.FindFirst("sub")?.Value,
                out int idUsuario))
            {
                return false;
            }

            return await _context.Empresas.AnyAsync(e =>
                e.IdEmpresa == id &&
                e.IdUsuario == idUsuario);
        }

        // ==========================================
        // LISTAR EMPRESAS
        // Exclusivo del administrador.
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>>
            Listar([FromQuery] bool? aprobada)
        {
            var consulta = _context.Empresas.AsNoTracking();

            if (aprobada.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.EstadoAprobacion == aprobada.Value);
            }

            var lista = await consulta
                .OrderBy(e => e.NombreComercial)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // ==========================================
        // CONSULTAR EMPRESA PROPIA
        // ==========================================

        [Authorize(Roles = Administrador + ",Empresa")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmpresaDto>>
            ObtenerPorId(int id)
        {
            if (!await PuedeAccederEmpresa(id))
                return Forbid();

            var empresa = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.IdEmpresa == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (empresa is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la empresa con id {id}."
                });
            }

            return Ok(empresa);
        }

        // ==========================================
        // REGISTRAR EMPRESA
        // Usuario y empresa en una transacción.
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpPost]
        public async Task<ActionResult<EmpresaDto>> Registrar(
            [FromBody] EmpresaRegistroDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.NombreRol == NombreRolEmpresa);

            if (rol is null)
            {
                return StatusCode(500, new
                {
                    mensaje = "No existe el rol Empresa."
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
                // Crear usuario inicialmente inactivo.

                var usuario = new Usuario
                {
                    IdRol = rol.IdRol,
                    Correo = correo,
                    EstadoCuenta = false
                };

                usuario.ContrasenaHash =
                    _hasher.HashPassword(
                        usuario,
                        dto.Contrasena);

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();

                // Crear empresa pendiente de aprobación.

                var empresa = new Empresa
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreComercial = dto.NombreComercial.Trim(),
                    RazonSocial = dto.RazonSocial.Trim(),
                    TelefonoContacto = dto.TelefonoContacto.Trim(),
                    EstadoAprobacion = false
                };

                _context.Empresas.Add(empresa);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                var respuesta = new EmpresaDto
                {
                    IdEmpresa = empresa.IdEmpresa,
                    IdUsuario = usuario.IdUsuario,
                    Correo = usuario.Correo,
                    NombreComercial = empresa.NombreComercial,
                    RazonSocial = empresa.RazonSocial,
                    TelefonoContacto = empresa.TelefonoContacto,
                    EstadoAprobacion = empresa.EstadoAprobacion
                };

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = empresa.IdEmpresa },
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
        // ACTUALIZAR EMPRESA PROPIA
        // ==========================================

        [Authorize(Roles = Administrador + ",Empresa")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmpresaDto>> Actualizar(
            int id,
            [FromBody] EmpresaActualizarDto dto)
        {
            if (!await PuedeAccederEmpresa(id))
                return Forbid();

            var empresa =
                await _context.Empresas.FindAsync(id);

            if (empresa is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la empresa con id {id}."
                });
            }

            empresa.NombreComercial =
                dto.NombreComercial.Trim();

            empresa.RazonSocial =
                dto.RazonSocial.Trim();

            empresa.TelefonoContacto =
                dto.TelefonoContacto.Trim();

            await _context.SaveChangesAsync();

            var respuesta = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.IdEmpresa == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // ==========================================
        // RF-14: APROBAR O DESACTIVAR EMPRESA
        // Exclusivo del administrador.
        // ==========================================

        [Authorize(Roles = Administrador)]
        [HttpPut("{id:int}/aprobacion")]
        public async Task<ActionResult<EmpresaDto>>
            CambiarAprobacion(
                int id,
                [FromBody] AprobacionEmpresaDto dto)
        {
            var empresa = await _context.Empresas
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e =>
                    e.IdEmpresa == id);

            if (empresa is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la empresa con id {id}."
                });
            }

            empresa.EstadoAprobacion = dto.Aprobada!.Value;

            // Activar o desactivar también el usuario.

            empresa.Usuario.EstadoCuenta =
                empresa.EstadoAprobacion;

            // Ambos cambios quedan en una misma transacción.
            await _context.SaveChangesAsync();

            var respuesta = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.IdEmpresa == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }
    }
}

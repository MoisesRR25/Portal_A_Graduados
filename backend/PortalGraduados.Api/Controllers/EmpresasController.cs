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
    public class EmpresasController : ControllerBase
    {
        private const string NombreRolEmpresa = "Empresa";

        private readonly PortalGraduadosDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public EmpresasController(PortalGraduadosDbContext context, IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        private static readonly Expression<Func<Empresa, EmpresaDto>> Proyeccion = e => new EmpresaDto
        {
            IdEmpresa = e.IdEmpresa,
            IdUsuario = e.IdUsuario,
            Correo = e.Usuario.Correo,
            NombreComercial = e.NombreComercial,
            RazonSocial = e.RazonSocial,
            TelefonoContacto = e.TelefonoContacto,
            EstadoAprobacion = e.EstadoAprobacion
        };

        // GET /api/empresas?aprobada=false
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> Listar([FromQuery] bool? aprobada)
        {
            var consulta = _context.Empresas.AsNoTracking();

            if (aprobada.HasValue)
                consulta = consulta.Where(e => e.EstadoAprobacion == aprobada.Value);

            var lista = await consulta
                .OrderBy(e => e.NombreComercial)
                .Select(Proyeccion)
                .ToListAsync();

            return Ok(lista);
        }

        // GET /api/empresas/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmpresaDto>> ObtenerPorId(int id)
        {
            var empresa = await _context.Empresas.AsNoTracking()
                .Where(e => e.IdEmpresa == id)
                .Select(Proyeccion)
                .FirstOrDefaultAsync();

            if (empresa is null)
                return NotFound(new { mensaje = $"No existe la empresa con id {id}." });

            return Ok(empresa);
        }

        // POST /api/empresas  (registro: usuario + empresa en una transaccion)
        [HttpPost]
        public async Task<ActionResult<EmpresaDto>> Registrar([FromBody] EmpresaRegistroDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            var rol = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.NombreRol == NombreRolEmpresa);

            if (rol is null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = $"No existe el rol '{NombreRolEmpresa}'. Cree primero los roles base." });

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

                // Paso 2: crear la empresa, pendiente de aprobacion
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

                return CreatedAtAction(nameof(ObtenerPorId), new { id = empresa.IdEmpresa }, respuesta);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                await transaccion.RollbackAsync();
                return Conflict(new { mensaje = "Ya existe un usuario con ese correo." });
            }
            // Cualquier otra excepcion sale del bloque sin CommitAsync:
            // al liberar la transaccion, PostgreSQL revierte todo (ROLLBACK).
        }

        // PUT /api/empresas/1
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmpresaDto>> Actualizar(int id, [FromBody] EmpresaActualizarDto dto)
        {
            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa is null)
                return NotFound(new { mensaje = $"No existe la empresa con id {id}." });

            empresa.NombreComercial = dto.NombreComercial.Trim();
            empresa.RazonSocial = dto.RazonSocial.Trim();
            empresa.TelefonoContacto = dto.TelefonoContacto.Trim();

            await _context.SaveChangesAsync();

            var respuesta = await _context.Empresas.AsNoTracking()
                .Where(e => e.IdEmpresa == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }

        // PUT /api/empresas/1/aprobacion
        [HttpPut("{id:int}/aprobacion")]
        public async Task<ActionResult<EmpresaDto>> CambiarAprobacion(int id, [FromBody] AprobacionEmpresaDto dto)
        {
            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa is null)
                return NotFound(new { mensaje = $"No existe la empresa con id {id}." });

            empresa.EstadoAprobacion = dto.Aprobada!.Value;
            await _context.SaveChangesAsync();

            var respuesta = await _context.Empresas.AsNoTracking()
                .Where(e => e.IdEmpresa == id)
                .Select(Proyeccion)
                .FirstAsync();

            return Ok(respuesta);
        }
    }
}
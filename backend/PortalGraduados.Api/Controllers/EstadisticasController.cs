
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;

namespace PortalGraduados.Api.Controllers
{
    [Authorize(Roles = "Administrador de la Escuela")]
    [ApiController]
    [Route("api/[controller]")]
    public class EstadisticasController : ControllerBase
    {
        private readonly PortalGraduadosDbContext _context;

        public EstadisticasController(
            PortalGraduadosDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // RF-12: RESUMEN ADMINISTRATIVO
        // ==========================================

        [HttpGet]
        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen()
        {
            var ahora = DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified);

            var totalGraduados =
                await _context.Graduados.CountAsync();

            var graduadosVerificados =
                await _context.Graduados.CountAsync(g =>
                    g.Verificado &&
                    g.Consentimiento &&
                    g.Usuario.EstadoCuenta);

            var graduadosPendientes =
                totalGraduados - graduadosVerificados;

            var totalEmpresas =
                await _context.Empresas.CountAsync();

            var empresasActivas =
                await _context.Empresas.CountAsync(e =>
                    e.EstadoAprobacion &&
                    e.Usuario.EstadoCuenta);

            var empresasPendientes =
                await _context.Empresas.CountAsync(e =>
                    !e.EstadoAprobacion);

            var totalOfertas =
                await _context.Ofertas.CountAsync();

            var ofertasActivas =
                await _context.Ofertas.CountAsync(o =>
                    o.Estado == "Publicada" &&
                    o.FechaCierre > ahora &&
                    o.Empresa.EstadoAprobacion &&
                    o.Empresa.Usuario.EstadoCuenta);

            var ofertasPendientes =
                await _context.Ofertas.CountAsync(o =>
                    o.Estado == "En revision");

            var ofertasCerradas =
                await _context.Ofertas.CountAsync(o =>
                    o.Estado == "Cerrada");

            var ofertasVencidas =
                await _context.Ofertas.CountAsync(o =>
                    o.Estado == "Publicada" &&
                    o.FechaCierre <= ahora);

            var totalPostulaciones =
                await _context.Postulaciones.CountAsync();

            var totalReportes =
                await _context.ReportesOferta.CountAsync();

            var totalCargas =
                await _context.CargasPerfiles.CountAsync();

            // ==========================================
            // ESTADÍSTICAS DE EMPLEABILIDAD
            // ==========================================

            var empleados =
                await _context.Graduados.CountAsync(g =>
                    g.EstadoLaboral.ToLower() == "empleado");

            var desempleados =
                await _context.Graduados.CountAsync(g =>
                    g.EstadoLaboral.ToLower() == "desempleado");

            var totalConEstadoConocido =
                empleados + desempleados;

            var tasaEmpleabilidad =
                totalConEstadoConocido == 0
                    ? 0
                    : Math.Round(
                        empleados * 100.0 /
                        totalConEstadoConocido, 2);

            return Ok(new
            {
                totalGraduados,
                graduadosVerificados,
                graduadosPendientes,

                totalEmpresas,
                empresasActivas,
                empresasPendientes,

                totalOfertas,
                ofertasActivas,
                ofertasPendientes,
                ofertasCerradas,
                ofertasVencidas,

                totalPostulaciones,
                totalReportes,
                totalCargas,

                empleados,
                desempleados,

                otrosEstadosLaborales =
                    totalGraduados - totalConEstadoConocido,

                tasaEmpleabilidad,

                fechaConsulta = DateTime.UtcNow
            });
        }

        // ==========================================
        // ESTADÍSTICAS POR ESTADO ACADÉMICO
        // ==========================================

        [HttpGet("estado-academico")]
        public async Task<IActionResult> PorEstadoAcademico()
        {
            var resultado = await _context.Graduados
                .AsNoTracking()
                .GroupBy(g => g.EstadoAcademico)
                .Select(grupo => new
                {
                    estado = grupo.Key,
                    total = grupo.Count()
                })
                .OrderByDescending(g => g.total)
                .ToListAsync();

            return Ok(resultado);
        }

        // ==========================================
        // ESTADÍSTICAS POR ESTADO LABORAL
        // ==========================================

        [HttpGet("estado-laboral")]
        public async Task<IActionResult> PorEstadoLaboral()
        {
            var resultado = await _context.Graduados
                .AsNoTracking()
                .GroupBy(g => g.EstadoLaboral)
                .Select(grupo => new
                {
                    estado = grupo.Key,
                    total = grupo.Count()
                })
                .OrderByDescending(g => g.total)
                .ToListAsync();

            return Ok(resultado);
        }

        // ==========================================
        // ESTADÍSTICAS POR ÁREA PROFESIONAL
        // ==========================================

        [HttpGet("areas")]
        public async Task<IActionResult> PorArea()
        {
            var resultado = await _context.Graduados
                .AsNoTracking()
                .GroupBy(g => g.AreaTrabajo)
                .Select(grupo => new
                {
                    area = grupo.Key,
                    total = grupo.Count()
                })
                .OrderByDescending(g => g.total)
                .ToListAsync();

            return Ok(resultado);
        }

        // ==========================================
        // ESTADÍSTICAS POR AÑO DE GRADUACIÓN
        // ==========================================

        [HttpGet("anios")]
        public async Task<IActionResult> PorAnio()
        {
            var resultado = await _context.Graduados
                .AsNoTracking()
                .GroupBy(g => g.AnioGraduacion)
                .Select(grupo => new
                {
                    anio = grupo.Key,
                    total = grupo.Count()
                })
                .OrderBy(g => g.anio)
                .ToListAsync();

            return Ok(resultado);
        }

        // ==========================================
        // POSTULACIONES POR ESTADO
        // ==========================================

        [HttpGet("postulaciones")]
        public async Task<IActionResult> PorPostulacion()
        {
            var resultado = await _context.Postulaciones
                .AsNoTracking()
                .GroupBy(p => p.Estado)
                .Select(grupo => new
                {
                    estado = grupo.Key,
                    total = grupo.Count()
                })
                .OrderByDescending(p => p.total)
                .ToListAsync();

            return Ok(resultado);
        }

        // ==========================================
        // POSTULACIONES POR OFERTA
        // ==========================================

        [HttpGet("ofertas")]
        public async Task<IActionResult> PorOferta(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 20)
        {
            if (pagina < 1 ||
                tamano < 1 ||
                tamano > 100)
            {
                return BadRequest(new
                {
                    mensaje = "Página o tamaño inválidos."
                });
            }

            var resultado = await _context.Ofertas
                .AsNoTracking()
                .Select(o => new
                {
                    o.IdOferta,
                    o.Cargo,

                    empresa =
                        o.Empresa.NombreComercial,

                    o.Estado,

                    totalPostulaciones =
                        _context.Postulaciones.Count(p =>
                            p.IdOferta == o.IdOferta)
                })
                .OrderByDescending(o =>
                    o.totalPostulaciones)
                .ThenBy(o => o.IdOferta)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return Ok(resultado);
        }
    }
}

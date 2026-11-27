using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PortalGraduados.Api.Data
{
    public class PortalGraduadosDbContextFactory : IDesignTimeDbContextFactory<PortalGraduadosDbContext>
    {
        public PortalGraduadosDbContext CreateDbContext(string[] args)
        {
            var configuracion = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets<PortalGraduadosDbContextFactory>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cadena = configuracion.GetConnectionString("PortalGraduados")
                ?? "Host=127.0.0.1;Port=5438;Database=portal_graduados;Username=portal_admin;Password=portal_admin";

            var opciones = new DbContextOptionsBuilder<PortalGraduadosDbContext>()
                .UseNpgsql(cadena)
                .Options;

            return new PortalGraduadosDbContext(opciones);
        }
    }
}

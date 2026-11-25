
using Microsoft.EntityFrameworkCore;

namespace PortalGraduados.Api.Data
{
    public class PortalGraduadosDbContext : DbContext
    {
        public PortalGraduadosDbContext(
            DbContextOptions<PortalGraduadosDbContext> options
        ) : base(options)
        {
        }
    }
}

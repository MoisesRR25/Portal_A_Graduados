
namespace PortalGraduados.Api.Models
{
    public class GraduadoHabilidad
    {
        // Identificador del graduado
        public int IdGraduado { get; set; }

        // Identificador de la habilidad
        public int IdHabilidad { get; set; }

        // Relación con Graduado
        public Graduado Graduado { get; set; } = null!;

        // Relación con Habilidad
        public Habilidad Habilidad { get; set; } = null!;
    }
}

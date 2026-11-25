
namespace PortalGraduados.Api.Models
{
    public class ExperienciaLaboral
    {
        public int IdExperiencia { get; set; }

        public int IdGraduado { get; set; }

        // Información de la experiencia laboral
        public string Empresa { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        // Fechas de la experiencia laboral
        public DateOnly FechaInicio { get; set; }

        public DateOnly? FechaFin { get; set; }

        // Relación con Graduado
        public Graduado Graduado { get; set; } = null!;
    }
}

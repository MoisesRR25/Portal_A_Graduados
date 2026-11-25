
namespace PortalGraduados.Api.Models
{
    public class Graduado
    {
        public int IdGraduado { get; set; }

        public int IdUsuario { get; set; }

        // Datos personales
        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        // Información académica
        public int AnioGraduacion { get; set; }

        public string EstadoAcademico { get; set; } = string.Empty;

        // Información laboral
        public string EstadoLaboral { get; set; } = string.Empty;

        public string AreaTrabajo { get; set; } = string.Empty;

        public string Ubicacion { get; set; } = string.Empty;

        // Documentos y portafolio
        public string? CvAdjunto { get; set; }

        public string? PortafolioEnlace { get; set; }

        // Verificación del perfil
        public bool Verificado { get; set; } = false;

        public bool Consentimiento { get; set; } = false;

        public DateTime FechaUltimaActualizacion { get; set; }

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;
    }
}

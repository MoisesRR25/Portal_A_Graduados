
namespace PortalGraduados.Api.Models
{
    public class Proyecto
    {
        public int IdProyecto { get; set; }

        public int IdGraduado { get; set; }

        // Información del proyecto
        public string NombreProyecto { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? TecnologiasUtilizadas { get; set; }

        public string? UrlProyecto { get; set; }

        // Relación con Graduado
        public Graduado Graduado { get; set; } = null!;
    }
}

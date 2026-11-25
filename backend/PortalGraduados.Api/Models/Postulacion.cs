
namespace PortalGraduados.Api.Models
{
    public class Postulacion
    {
        public int IdPostulacion { get; set; }

        // Identificadores de las relaciones
        public int IdGraduado { get; set; }

        public int IdOferta { get; set; }

        // Información de la postulación
        public DateTime FechaPostulacion { get; set; }

        public string CvEnviado { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        // Relación con Graduado
        public Graduado Graduado { get; set; } = null!;

        // Relación con Oferta
        public Oferta Oferta { get; set; } = null!;
    }
}

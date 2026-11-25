
namespace PortalGraduados.Api.Models
{
    public class ReporteOferta
    {
        public int IdReporte { get; set; }

        // Identificadores de las relaciones
        public int IdUsuario { get; set; }

        public int IdOferta { get; set; }

        // Información del reporte
        public string Motivo { get; set; } = string.Empty;

        public DateTime FechaReporte { get; set; }

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;

        // Relación con Oferta
        public Oferta Oferta { get; set; } = null!;
    }
}

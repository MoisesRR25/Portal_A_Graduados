
namespace PortalGraduados.Api.Models
{
    public class OfertaHabilidad
    {
        // Identificador de la oferta
        public int IdOferta { get; set; }

        // Identificador de la habilidad
        public int IdHabilidad { get; set; }

        // Relación con Oferta
        public Oferta Oferta { get; set; } = null!;

        // Relación con Habilidad
        public Habilidad Habilidad { get; set; } = null!;
    }
}


namespace PortalGraduados.Api.Models
{
    public class Oferta
    {
        public int IdOferta { get; set; }

        public int IdEmpresa { get; set; }

        // Información de la oferta laboral
        public string Cargo { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public string Ubicacion { get; set; } = string.Empty;

        public string DescripcionCorta { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Requisitos { get; set; } = string.Empty;

        // Condiciones de la oferta
        public string? RangoSalarial { get; set; }

        public string Modalidad { get; set; } = string.Empty;

        public string ClasificacionOportunidad { get; set; } = string.Empty;

        // Fechas de la oferta
        public DateTime FechaPublicacion { get; set; }

        public DateTime FechaCierre { get; set; }

        // Estado de la oferta
        public string Estado { get; set; } = string.Empty;

        // Relación con Empresa
        public Empresa Empresa { get; set; } = null!;
    }
}

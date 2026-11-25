
namespace PortalGraduados.Api.Models
{
    public class CargaPerfiles
    {
        public int IdCarga { get; set; }

        // Usuario que realiza la carga masiva
        public int IdUsuario { get; set; }

        // Fecha de la carga
        public DateTime FechaCarga { get; set; }

        // Resultados de la carga
        public int TotalRegistros { get; set; }

        public int TotalCargados { get; set; }

        public int TotalRechazados { get; set; }

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;
    }
}


namespace PortalGraduados.Api.Models
{
    public class Habilidad
    {
        public int IdHabilidad { get; set; }

        // Información de la habilidad
        public string NombreHabilidad { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;
    }
}

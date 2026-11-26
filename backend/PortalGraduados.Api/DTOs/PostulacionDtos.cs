using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    public class PostulacionDto
    {
        public int IdPostulacion { get; set; }
        public int IdGraduado { get; set; }
        public string NombreGraduado { get; set; } = string.Empty;
        public int IdOferta { get; set; }
        public string CargoOferta { get; set; } = string.Empty;
        public int IdEmpresa { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public DateTime FechaPostulacion { get; set; }
        public string CvEnviado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class PostulacionCrearDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el id del graduado.")]
        public int IdGraduado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el id de la oferta.")]
        public int IdOferta { get; set; }

        // Opcional: si no viene, se usa el CV del perfil del graduado
        [StringLength(255, ErrorMessage = "El CV no puede superar 255 caracteres.")]
        public string? CvEnviado { get; set; }
    }

    public class CambioEstadoPostulacionDto
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [AllowedValues("En revision", "Aceptada", "Rechazada",
            ErrorMessage = "El estado debe ser En revision, Aceptada o Rechazada.")]
        public string Estado { get; set; } = string.Empty;
    }
}
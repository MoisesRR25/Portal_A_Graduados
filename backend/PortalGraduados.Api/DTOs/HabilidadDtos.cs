using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    // Lo que la API devuelve
    public class HabilidadDto
    {
        public int IdHabilidad { get; set; }
        public string NombreHabilidad { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
    }

    // Lo que la API recibe en POST y PUT
    public class HabilidadRequestDto
    {
        [Required(ErrorMessage = "El nombre de la habilidad es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string NombreHabilidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoria es obligatoria.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "La categoria debe tener entre 2 y 50 caracteres.")]
        public string Categoria { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    public class RolDto
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class RolRequestDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string NombreRol { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La descripcion no puede superar 100 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
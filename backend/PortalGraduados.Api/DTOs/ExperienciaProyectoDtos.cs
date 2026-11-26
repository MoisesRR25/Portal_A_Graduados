using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    // ----- Experiencia laboral -----
    public class ExperienciaDto
    {
        public int IdExperiencia { get; set; }
        public int IdGraduado { get; set; }
        public string Empresa { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
    }

    public class ExperienciaRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "La empresa es obligatoria.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "La empresa debe tener entre 2 y 150 caracteres.")]
        public string Empresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El cargo debe tener entre 2 y 100 caracteres.")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripcion es obligatoria.")]
        [StringLength(2000, ErrorMessage = "La descripcion no puede superar 2000 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateOnly? FechaInicio { get; set; }

        // Null significa que es su empleo actual
        public DateOnly? FechaFin { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaInicio.HasValue && FechaFin.HasValue && FechaFin.Value < FechaInicio.Value)
            {
                yield return new ValidationResult(
                    "La fecha de fin no puede ser anterior a la fecha de inicio.",
                    new[] { nameof(FechaFin) });
            }
        }
    }

    // ----- Proyecto -----
    public class ProyectoDto
    {
        public int IdProyecto { get; set; }
        public int IdGraduado { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? TecnologiasUtilizadas { get; set; }
        public string? UrlProyecto { get; set; }
    }

    public class ProyectoRequestDto
    {
        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
        public string NombreProyecto { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "La descripcion no puede superar 2000 caracteres.")]
        public string? Descripcion { get; set; }

        [StringLength(2000, ErrorMessage = "Las tecnologias no pueden superar 2000 caracteres.")]
        public string? TecnologiasUtilizadas { get; set; }

        [StringLength(255, ErrorMessage = "La URL no puede superar 255 caracteres.")]
        [Url(ErrorMessage = "La URL del proyecto no es valida.")]
        public string? UrlProyecto { get; set; }
    }
}
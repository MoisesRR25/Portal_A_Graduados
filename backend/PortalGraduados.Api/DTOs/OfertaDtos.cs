using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    public class OfertaDto
    {
        public int IdOferta { get; set; }
        public int IdEmpresa { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string DescripcionCorta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Requisitos { get; set; } = string.Empty;
        public string? RangoSalarial { get; set; }
        public string Modalidad { get; set; } = string.Empty;
        public string ClasificacionOportunidad { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public DateTime FechaCierre { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class OfertaDetalleDto
    {
        public OfertaDto Oferta { get; set; } = new();
        public List<HabilidadDto> Habilidades { get; set; } = new();
    }

    // Datos editables de la oferta (no incluye empresa, estado ni fecha de publicacion)
    public class OfertaActualizarDto
    {
        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "El cargo debe tener entre 2 y 150 caracteres.")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El area es obligatoria.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El area debe tener entre 2 y 100 caracteres.")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicacion es obligatoria.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "La ubicacion debe tener entre 2 y 100 caracteres.")]
        public string Ubicacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripcion corta es obligatoria.")]
        [StringLength(100, MinimumLength = 5,
            ErrorMessage = "La descripcion corta debe tener entre 5 y 100 caracteres.")]
        public string DescripcionCorta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripcion es obligatoria.")]
        [StringLength(5000, MinimumLength = 10,
            ErrorMessage = "La descripcion debe tener entre 10 y 5000 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los requisitos son obligatorios.")]
        [StringLength(5000, MinimumLength = 10,
            ErrorMessage = "Los requisitos deben tener entre 10 y 5000 caracteres.")]
        public string Requisitos { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El rango salarial no puede superar 50 caracteres.")]
        public string? RangoSalarial { get; set; }

        [Required(ErrorMessage = "La modalidad es obligatoria.")]
        [AllowedValues("Presencial", "Remoto", "Hibrido",
            ErrorMessage = "La modalidad debe ser Presencial, Remoto o Hibrido.")]
        public string Modalidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clasificacion de la oportunidad es obligatoria.")]
        [StringLength(30, MinimumLength = 2,
            ErrorMessage = "La clasificacion debe tener entre 2 y 30 caracteres.")]
        public string ClasificacionOportunidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de cierre es obligatoria.")]
        public DateTime? FechaCierre { get; set; }
    }

    // Publicar: los mismos datos mas la empresa que publica
    public class OfertaCrearDto : OfertaActualizarDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el id de la empresa.")]
        public int IdEmpresa { get; set; }
    }

        public class RevisionOfertaDto : IValidatableObject
    {
        [Required(ErrorMessage = "Debe indicar si la oferta se aprueba (true) o se rechaza (false).")]
        public bool? Aprobada { get; set; }

        [StringLength(500, ErrorMessage = "El motivo no puede superar 500 caracteres.")]
        public string? Motivo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Aprobada == false && string.IsNullOrWhiteSpace(Motivo))
            {
                yield return new ValidationResult(
                    "Debe indicar el motivo del rechazo.",
                    new[] { nameof(Motivo) });
            }
        }
    }
}
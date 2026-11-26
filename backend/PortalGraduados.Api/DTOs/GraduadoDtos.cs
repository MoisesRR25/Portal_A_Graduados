using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    // Respuesta: nunca incluye la contrasena ni su hash
    public class GraduadoDto
    {
        public int IdGraduado { get; set; }
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int AnioGraduacion { get; set; }
        public string EstadoAcademico { get; set; } = string.Empty;
        public string EstadoLaboral { get; set; } = string.Empty;
        public string AreaTrabajo { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string? CvAdjunto { get; set; }
        public string? PortafolioEnlace { get; set; }
        public bool Verificado { get; set; }
        public bool Consentimiento { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
    }

    public class GraduadoDetalleDto
    {
        public GraduadoDto Graduado { get; set; } = new();
        public List<HabilidadDto> Habilidades { get; set; } = new();
    }

    // Campos del perfil, comunes al registro y a la actualizacion
    public class GraduadoPerfilDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono es obligatorio.")]
        [StringLength(20, MinimumLength = 8,
            ErrorMessage = "El telefono debe tener entre 8 y 20 caracteres.")]
        public string Telefono { get; set; } = string.Empty;

        [Range(1950, 2100, ErrorMessage = "El anio de graduacion debe estar entre 1950 y 2100.")]
        public int AnioGraduacion { get; set; }

        [Required(ErrorMessage = "El estado academico es obligatorio.")]
        [StringLength(30, MinimumLength = 2,
            ErrorMessage = "El estado academico debe tener entre 2 y 30 caracteres.")]
        public string EstadoAcademico { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado laboral es obligatorio.")]
        [StringLength(30, MinimumLength = 2,
            ErrorMessage = "El estado laboral debe tener entre 2 y 30 caracteres.")]
        public string EstadoLaboral { get; set; } = string.Empty;

        [Required(ErrorMessage = "El area de trabajo es obligatoria.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El area de trabajo debe tener entre 2 y 100 caracteres.")]
        public string AreaTrabajo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicacion es obligatoria.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "La ubicacion debe tener entre 2 y 100 caracteres.")]
        public string Ubicacion { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "El CV no puede superar 255 caracteres.")]
        public string? CvAdjunto { get; set; }

        [StringLength(255, ErrorMessage = "El enlace no puede superar 255 caracteres.")]
        [Url(ErrorMessage = "El enlace del portafolio debe ser una URL valida (http o https).")]
        public string? PortafolioEnlace { get; set; }
    }

    // Registro: crea usuario y perfil a la vez
    public class GraduadoRegistroDto : GraduadoPerfilDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "La contrasena debe tener entre 8 y 100 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        public bool Consentimiento { get; set; }
    }

    // Actualizacion: solo datos del perfil (no toca correo, contrasena ni verificacion)
    public class GraduadoActualizarDto : GraduadoPerfilDto
    {
    }

    public class VerificacionGraduadoDto
    {
        [Required(ErrorMessage = "Debe indicar si el perfil queda verificado (true o false).")]
        public bool? Verificado { get; set; }
    }
}
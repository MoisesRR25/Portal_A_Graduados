using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    // Respuesta: nunca incluye la contraseña ni su hash
    public class EmpresaDto
    {
        public int IdEmpresa { get; set; }
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string TelefonoContacto { get; set; } = string.Empty;
        public bool EstadoAprobacion { get; set; }
    }

    // Registro: crea usuario y empresa a la vez
    public class EmpresaRegistroDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "La contrasena debe tener entre 8 y 100 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es obligatorio.")]
        [StringLength(150, MinimumLength = 2)]
        public string NombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razon social es obligatoria.")]
        [StringLength(250, MinimumLength = 2)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono de contacto es obligatorio.")]
        [StringLength(20, MinimumLength = 8,
            ErrorMessage = "El telefono debe tener entre 8 y 20 caracteres.")]
        public string TelefonoContacto { get; set; } = string.Empty;
    }

    // Actualización de datos de la empresa (no toca correo ni contraseña)
    public class EmpresaActualizarDto
    {
        [Required(ErrorMessage = "El nombre comercial es obligatorio.")]
        [StringLength(150, MinimumLength = 2)]
        public string NombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razon social es obligatoria.")]
        [StringLength(250, MinimumLength = 2)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono de contacto es obligatorio.")]
        [StringLength(20, MinimumLength = 8,
            ErrorMessage = "El telefono debe tener entre 8 y 20 caracteres.")]
        public string TelefonoContacto { get; set; } = string.Empty;
    }

    public class AprobacionEmpresaDto
    {
        [Required(ErrorMessage = "Debe indicar si la empresa queda aprobada (true o false).")]
        public bool? Aprobada { get; set; }
    }
}
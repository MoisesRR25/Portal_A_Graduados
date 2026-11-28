
using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(128)]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class UsuarioAutenticadoDto
    {
        public int IdUsuario { get; set; }

        public string Correo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public int? IdGraduado { get; set; }

        public int? IdEmpresa { get; set; }
    }

    public class LoginRespuestaDto
    {
        public string Token { get; set; } = string.Empty;

        public string TipoToken { get; set; } = "Bearer";

        public DateTime Expiracion { get; set; }

        public UsuarioAutenticadoDto Usuario { get; set; } = new();
    }
}

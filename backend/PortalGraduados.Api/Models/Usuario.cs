
namespace PortalGraduados.Api.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Correo { get; set; } = string.Empty;

        public string ContrasenaHash { get; set; } = string.Empty;

        public bool EstadoCuenta { get; set; }

        // Relación con la entidad Rol
        public Rol Rol { get; set; } = null!;
    }
}

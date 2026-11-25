
namespace PortalGraduados.Api.Models
{
    public class Empresa
    {
        public int IdEmpresa { get; set; }

        public int IdUsuario { get; set; }

        // Información de la empresa
        public string NombreComercial { get; set; } = string.Empty;

        public string RazonSocial { get; set; } = string.Empty;

        public string TelefonoContacto { get; set; } = string.Empty;

        // Estado de aprobación
        public bool EstadoAprobacion { get; set; } = false;

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;
    }
}

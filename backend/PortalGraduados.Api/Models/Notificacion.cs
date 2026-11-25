
namespace PortalGraduados.Api.Models
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }

        // Usuario que recibe la notificación
        public int IdUsuario { get; set; }

        // Información de la notificación
        public string Tipo { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public bool Leida { get; set; } = false;

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;
    }
}

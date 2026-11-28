
using System.ComponentModel.DataAnnotations;

namespace PortalGraduados.Api.DTOs
{
    public class ReporteOfertaCrearDto
    {
        [Range(1, int.MaxValue)]
        public int IdOferta { get; set; }

        [Required(
            ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage =
                "El motivo debe tener entre 10 y 1000 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }

    public class ReporteOfertaDto
    {
        public int IdReporte { get; set; }

        public int IdUsuario { get; set; }

        public int IdOferta { get; set; }

        public string Cargo { get; set; } = string.Empty;

        public string Empresa { get; set; } = string.Empty;

        public string Motivo { get; set; } = string.Empty;

        public DateTime FechaReporte { get; set; }

        public string EstadoOferta { get; set; } = string.Empty;
    }
}


namespace PortalGraduados.Api.DTOs
{
    // Registro recibido desde el Excel de Angular.

    public class RegistroCargaDto
    {
        public int Fila { get; set; }

        public string? Nombre { get; set; }

        public string? Correo { get; set; }

        public string? Carnet { get; set; }

        public string? Anio { get; set; }

        public string? EstadoLaboral { get; set; }
    }

    // Solicitud de carga masiva.

    public class CargaMasivaRequestDto
    {
        public List<RegistroCargaDto?> Registros { get; set; } = new();
    }

    // Resultado individual de cada registro.

    public class ResultadoRegistroCargaDto
    {
        public int Fila { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public string Anio { get; set; } = string.Empty;

        public string EstadoLaboral { get; set; } = string.Empty;

        public List<string> Errores { get; set; } = new();

        public int? IdGraduado { get; set; }

        public string Estado =>
            Errores.Count == 0
                ? "Pendiente de activacion"
                : "Rechazado";
    }

    // Resultado completo de la carga.

    public class CargaMasivaRespuestaDto
    {
        public int IdCarga { get; set; }

        public DateTime FechaCarga { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalCargados { get; set; }

        public int TotalRechazados { get; set; }

        public List<ResultadoRegistroCargaDto>
            Resultados { get; set; } = new();
    }
}

namespace TelMedAPI.DTOs
{
    public class CitaResponseDTO
    {
        public int IdCita { get; set; }
        public DateTimeOffset FechaInicio { get; set; }
        public DateTimeOffset FechaFin { get; set; }
        public string Motivo { get; set; }
        public string TipoConsulta { get; set; }
        public string Estado { get; set; }
        public string? LinkReunion { get; set; }
    }
}
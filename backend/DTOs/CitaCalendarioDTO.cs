namespace TelMedAPI.DTOs
{
    public class CitaCalendarDto
    {
        public int IdCita { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string PacienteNombreCompleto { get; set; } = string.Empty;
        public string TipoConsulta { get; set; } = string.Empty;
        public int PacienteId { get; set; }
        public string? LinkReunion { get; set; }
        public string? DuiPaciente { get; set; }
        public string? TelefonoPaciente { get; set; }
        public string?  CorreoPaciente         { get; set; }
        public string?  FechaNacimientoPaciente { get; set; }
        public string?  FotoPaciente           { get; set; }
        public string DoctorNombreCompleto   {get; set;}
    }
}

namespace TelMedAPI.DTOs
{
    public class CreateCitaDTO
    {
         public int PacienteId { get; set; }
         public string Motivo { get; set; }
        public DateTimeOffset FechaInicio { get; set; }
        public DateTimeOffset FechaFin { get; set; }
        public string TipoConsulta { get; set; }
        public int DoctorId { get; set; }
        public string? LinkReunion { get; set; }
    }
}
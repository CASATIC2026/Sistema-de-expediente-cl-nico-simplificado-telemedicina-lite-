namespace TelMedAPI.DTOs
{
    public class CreateConsultaDTO
    {
        public int CitaId { get; set; }
        public string Sintomas { get; set; } = string.Empty;
        public string Evolucion { get; set; } = string.Empty;
        public string Diagnostico { get; set; } = string.Empty;
        public string Tratamiento { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public string MedicamentosJson { get; set; } = string.Empty;

        public bool TieneIncapacidad { get; set; }
        public DateTime? FechaInicioIncapacidad { get; set; }
        public DateTime? FechaFinIncapacidad { get; set; }
        public int? DiasIncapacidad { get; set; }
        public string MotivoIncapacidad { get; set; } = string.Empty;
        public string ObservacionesIncapacidad { get; set; } = string.Empty;
    }
}
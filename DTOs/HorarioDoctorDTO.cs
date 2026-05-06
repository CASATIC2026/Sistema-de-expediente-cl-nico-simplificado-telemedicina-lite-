namespace TelMedAPI.DTOs
{
    public class HorarioDoctorDTO
    {
        public int    DiaSemana  { get; set; }
        public string HoraInicio { get; set; } = "";
        public string HoraFin    { get; set; } = "";
        public bool   Activo     { get; set; }
    }
}
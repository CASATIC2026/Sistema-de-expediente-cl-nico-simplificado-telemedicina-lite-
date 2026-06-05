namespace TelMedAPI.DTOs
{
    public class UpdateClinicProfileDTO
    {
        public string ClinicName { get; set; } = string.Empty;
        public string Slogan { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}
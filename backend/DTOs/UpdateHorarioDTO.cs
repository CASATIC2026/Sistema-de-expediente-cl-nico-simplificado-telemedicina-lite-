namespace TelMedAPI.DTOs
{
    public class UpdateHorarioDTO
    {
        public int DiaSemana { get; set; }

        public bool Activo { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }
    }
}
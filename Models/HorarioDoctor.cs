using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelMedAPI.Models
{
    [Table("horario_doctor")]
    public class HorarioDoctor
    {
        [Key]
        [Column("id_horario")]
        public int IdHorario { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Usuario? Doctor { get; set; }

        [Column("dia_semana")]
        public int DiaSemana { get; set; }
        // 0 = Domingo
        // 1 = Lunes
        // 2 = Martes
        // etc.

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("hora_inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Column("hora_fin")]
        public TimeSpan HoraFin { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TelMedAPI.Models
{
    [Table("consulta")]
    [Index(nameof(CitaId), IsUnique = true)]
    public class Consulta
    {
        [Key]
        [Column("id_consulta")]
        public int IdConsulta { get; set; }

        [Column("cita_id")]
        public int CitaId { get; set; }

        [ForeignKey("CitaId")]
        public Cita Cita { get; set; } = null!;

        [Column("sintomas")]
        public string Sintomas { get; set; } = string.Empty;

        [Column("evolucion")]
        public string Evolucion { get; set; } = string.Empty;

        [Column("diagnostico")]
        public string Diagnostico { get; set; } = string.Empty;

        [Column("tratamiento")]
        public string Tratamiento { get; set; } = string.Empty;

        [Column("observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        // Se guardan medicamentos como Json
        [Column("medicamentos_json")]
        public string MedicamentosJson { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TelMedAPI.Models;

namespace TelMedAPI.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        public string? GoogleId { get; set; }
        public string? FotoUrl { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public DateOnly FechaNacimiento { get; set; }

        public string? DUI { get; set; }

        [Column("Direccion")] 
        public string Direccion { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;
        
        public string Telefono { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }

        public string Rol { get; set; } = string.Empty;

        public bool DebeCambiarPassword { get; set; } = false;

        public bool Eliminado { get; set; } = false;

        public bool Activo { get; set; } = true;

        public bool EmailVerified { get; set; } = false;
        
        // Verificación de email y recuperación de contraseña
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationExpiresAt { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }

        // Relaciones
        public ICollection<Cita>? CitasComoPaciente { get; set; }
        public ICollection<Cita>? CitasComoDoctor { get; set; }
    }
}
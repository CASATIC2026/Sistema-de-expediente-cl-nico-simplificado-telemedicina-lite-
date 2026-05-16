using System.ComponentModel.DataAnnotations;

namespace TelMedAPI.DTOs
{
    public class UpdatePerfilDTO
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Telefono { get; set; }

        public string? JVPM { get; set; }
        public string? Especialidad { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using TelMedAPI.Validators;

namespace TelMedAPI.DTOs
{
    public class RegisterDTO
    {
        // =========================================================
        // NOMBRE (solo letras y espacios)
        // =========================================================
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
            ErrorMessage = "El nombre solo debe contener letras."
        )]
        public string Nombre { get; set; }

        // =========================================================
        // APELLIDO (solo letras y espacios)
        // =========================================================
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
            ErrorMessage = "El apellido solo debe contener letras."
        )]
        public string Apellido { get; set; }

        // =========================================================
        // GÉNERO
        // =========================================================
        [Required(ErrorMessage = "El género es obligatorio.")]
        public string Genero { get; set; }

        // =========================================================
        // FECHA DE NACIMIENTO (Debe ser mayor de 18 años)
        // =========================================================
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [AdultValidator(ErrorMessage = "Debes ser mayor de 18 años para registrarte.")]
        public DateOnly FechaNacimiento { get; set; }

        // =========================================================
        // DIRECCIÓN
        // =========================================================
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; }

        // =========================================================
        // CORREO (Validación de formato email)
        // =========================================================
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo no válido.")]
        public string Email { get; set; }

        // =========================================================
        // CONTRASEÑA (mínimo 8 caracteres, 1 número y 1 símbolo)
        // =========================================================
       [RegularExpression(
        @"^(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, un número y un símbolo."
        )]
        public string? Password { get; set; }
        
        // =========================================================
        // TELÉFONO (Solo 8 números: 70007000)
        // =========================================================
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(
            @"^\d{8}$",
            ErrorMessage = "El teléfono debe tener exactamente 8 números."
        )]
        public string Telefono { get; set; }


        public string? JVPM { get; set; }
        public string? Especialidad { get; set; }

        // =========================================================
        // DUI (Formato: 00000000-0)
        // =========================================================
        [Required(ErrorMessage = "El DUI es obligatorio.")]
        [RegularExpression(
            @"^\d{8}-\d{1}$",
            ErrorMessage = "El DUI debe tener formato 00000000-0."
        )]
        public string DUI { get; set; }
    }
}
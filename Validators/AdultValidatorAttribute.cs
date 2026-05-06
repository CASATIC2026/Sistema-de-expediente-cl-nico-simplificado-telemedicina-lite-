using System.ComponentModel.DataAnnotations;

namespace TelMedAPI.Validators
{
    public class AdultValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(
                    "La fecha de nacimiento es obligatoria."
                );
            }

            if (value is not DateOnly fechaNacimiento)
            {
                return new ValidationResult(
                    "Formato de fecha inválido."
                );
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var edad = hoy.Year - fechaNacimiento.Year;

            // Ajuste si aún no ha cumplido años este año
            if (fechaNacimiento > hoy.AddYears(-edad))
            {
                edad--;
            }

            if (edad < 18)
            {
                return new ValidationResult(
                    ErrorMessage ??
                    "Debes ser mayor de 18 años para registrarte."
                );
            }

            return ValidationResult.Success;
        }
    }
}
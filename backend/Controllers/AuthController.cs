using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TelMedAPI.Data;
using TelMedAPI.Models;
using TelMedAPI.DTOs;
using TelMedAPI.Services;
using Microsoft.AspNetCore.Authorization;
using TelMedAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TelMedAPIContext _context;
    private readonly string _key;
    private readonly IConfiguration _config;
    private readonly EmailService _emailService;

    public AuthController(TelMedAPIContext context, IConfiguration config, EmailService emailService)
    {
        _context = context;
        _config = config;
        _key = config["Jwt:Key"] 
            ?? throw new ArgumentNullException("Jwt:Key no configurado en appsettings.json");
        _emailService = emailService;
    }

    // =========================================================
    // REGISTRO DE PACIENTE (Registro Normal)
    // =========================================================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var emailNormalizado = model.Email.Trim().ToLower();

        // Validación de duplicados
        if (await _context.Usuarios.AnyAsync(u => u.Email == emailNormalizado))
            return BadRequest(new
            {
                message = "El correo ya está registrado."
            });

        if (await _context.Usuarios.AnyAsync(u => u.DUI == model.DUI))
            return BadRequest(new
            {
                message = "El DUI ya está registrado."
            });

        //Validación de contraseña
        if (string.IsNullOrEmpty(model.Password))
        {
            return BadRequest(new
            {
                message = "La contraseña es obligatoria."
            });
        }

        var paciente = new Usuario
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            Genero = model.Genero,
            FechaNacimiento = model.FechaNacimiento,
            Direccion = model.Direccion,
            Email = emailNormalizado,
            Telefono = model.Telefono,
            DUI = model.DUI,
            Rol = Roles.Paciente,
            DebeCambiarPassword = false,

            // Verificación de email
            EmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString(),
            EmailVerificationExpiresAt = DateTime.UtcNow.AddHours(24),

            // Hash de contraseña
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        try
    {
        _context.Usuarios.Add(paciente);
        await _context.SaveChangesAsync();

        await _emailService.EnviarCorreoVerificacion(
            paciente.Email,
            paciente.EmailVerificationToken!
        );

        return Ok(new
        {
            message = "Registro exitoso. Revisa tu correo para verificar tu cuenta antes de iniciar sesión."
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en Register: {ex.Message}");

        return StatusCode(500, new
        {
            message = "Error interno al procesar el registro."
        });
    }
}

        // =========================================================
        // VERIFICACIÓN DE CORREO ("Activa" la cuenta del paciente)
        // =========================================================
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new
                {
                    message = "Token inválido."
                });

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.EmailVerificationToken == token);

            if (user == null)
                return BadRequest(new
                {
                    message = "Token no válido o usuario no encontrado."
                });

            if (user.EmailVerificationExpiresAt == null ||
                user.EmailVerificationExpiresAt < DateTime.UtcNow)
                return BadRequest(new
                {
                    message = "El enlace de verificación ha expirado."
                });

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Correo verificado correctamente. Ya puedes iniciar sesión."
            });
        }


            // =========================================================
            // COMPLETAR PERFIL (Para usuarios de Google)
            // =========================================================
            [Authorize]
            [HttpPost("completar-perfil")]
            public async Task<IActionResult> CompletarPerfil([FromBody] RegisterDTO model)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return NotFound("Usuario no encontrado");

                // Solo usuarios de Google
                if (user.GoogleId == null)
                {
                    return BadRequest(new { message = "Este endpoint es solo para usuarios de Google." });
                }

                // Contraseña opcional (solo si no tiene)
                if (!string.IsNullOrEmpty(model.Password))
                {
                    if (user.PasswordHash != null)
                    {
                        return BadRequest(new { message = "Ya tienes una contraseña definida." });
                    }

                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                // Actualización de campos
                user.Nombre = model.Nombre;
                user.Apellido = model.Apellido;
                user.Genero = model.Genero;
                user.FechaNacimiento = model.FechaNacimiento;
                user.Direccion = model.Direccion;
                user.Telefono = model.Telefono;
                user.DUI = model.DUI;

                await _context.SaveChangesAsync();

                var token = GenerarToken(user);

                return Ok(new {
                    token = token,
                    rol = user.Rol.ToLower(),
                    perfilCompleto = true,
                    message = "Perfil actualizado correctamente."
                });
            }

            // =========================================================
            // LOGIN TRADICIONAL
            // =========================================================
            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDTO login)
            {
                if (string.IsNullOrWhiteSpace(login.Email) ||
                    string.IsNullOrWhiteSpace(login.Password))
                {
                    return BadRequest(new
                    {
                        message = "Correo y contraseña son obligatorios."
                    });
                }

                var emailNormalizado = login.Email.Trim().ToLower();

                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == emailNormalizado);

                if (user == null) return Unauthorized("Credenciales inválidas.");

                if (user.GoogleId != null)
                    return BadRequest("Este usuario utiliza inicio de sesión con Google.");

                if (!user.EmailVerified)
                {
                    return BadRequest(new
                    {
                        message = "Debes verificar tu correo antes de iniciar sesión."
                    });
                }
                if (!user.Activo || user.Eliminado)
                    {
                        return Unauthorized(new
                        {
                            message = "Tu cuenta está inactiva o eliminada."
                        });
                    }

                if (user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                    return Unauthorized("Credenciales inválidas.");

                var token = GenerarToken(user);

                return Ok(new
                {
                    token,
                    id = user.Id,
                    rol = user.Rol.ToLower(),
                    perfilCompleto = PerfilCompleto(user),
                    requiereCambioPassword = user.DebeCambiarPassword
                });
            }

   // =========================================================
    // GOOGLE LOGIN
    // =========================================================
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleDTO dto)
    {
        try 
        {
            // Obtener el ClientId de la configuración
            var clientId = _config["Google:ClientId"];
            
            // Valida el token incluyendo la audiencia (ClientId)
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
<<<<<<< HEAD
=======
<<<<<<< Updated upstream
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == payload.Email);
=======
>>>>>>> backend
                Audience = new List<string> { clientId },
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.idToken, settings);
            
<<<<<<< HEAD
            // ... resto de tu lógica de búsqueda/creación de usuario ...
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == payload.Email);
=======
            var emailGoogle = payload.Email.Trim().ToLower();

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == emailGoogle);
>>>>>>> Stashed changes
>>>>>>> backend

                if (user == null)
                {
                    user = new Usuario
                    {
                        Nombre = payload.GivenName ?? payload.Name,
                        Apellido = payload.FamilyName ?? "",
                        Email = emailGoogle,
                        GoogleId = payload.Subject,
                        FotoUrl = payload.Picture,
                        Rol = Roles.Paciente,
                        DebeCambiarPassword = false,
                        EmailVerified = true
                    };
                    _context.Usuarios.Add(user);
                }
                else if (user.GoogleId == null)
                {
                    user.GoogleId = payload.Subject;
                }
                if (!user.Activo || user.Eliminado)
                    {
                        return Unauthorized(new
                        {
                            message = "Tu cuenta está inactiva o eliminada."
                        });
                    }

                await _context.SaveChangesAsync();
                var token = GenerarToken(user);

            return Ok(new {
                token,
                id = user.Id,
                rol = user.Rol.ToLower(),
                perfilCompleto = PerfilCompleto(user),
                requiereCambioPassword = user.DebeCambiarPassword
            });
        }
        catch (Exception)
        {
            return BadRequest("Token de Google inválido.");
        }
    }

    // =========================================================
    // RECUPERACIÓN DE CONTRASEÑA
    // =========================================================
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return BadRequest(new
            {
                message = "El correo es obligatorio."
            });
        }

        var emailNormalizado = model.Email.Trim().ToLower();

        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado);

        // Siempre responder lo mismo por seguridad
        if (user.GoogleId != null)
        {
            return Ok(new
            {
                message = "Si el correo existe, se enviará un enlace de recuperación."
            });
        }

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        try
        {
            await _emailService.EnviarCorreoRecuperacion(
                user.Email,
                user.PasswordResetToken!
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando correo de recuperación: {ex.Message}");

            // No revelar error técnico al usuario
            // para mantener seguridad y UX consistente
        }

        return Ok(new
        {
            message = "Si el correo existe, se enviará un enlace de recuperación."
        });
    }

    // =========================================================
    // REINICIO DE CONTRASEÑA (Usando el token enviado por correo)
    // =========================================================
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
    {
        if (string.IsNullOrEmpty(model.Token))
        {
            return BadRequest(new
            {
                message = "Token inválido."
            });
        }

        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.PasswordResetToken == model.Token);

        if (user == null)
        {
            return BadRequest(new
            {
                message = "Token inválido o usuario no encontrado."
            });
        }

        if (user.PasswordResetExpiresAt < DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "El enlace de recuperación ha expirado."
            });
        }

        if (string.IsNullOrWhiteSpace(model.NewPassword))
        {
            return BadRequest(new
            {
                message = "La nueva contraseña es obligatoria."
            });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Contraseña actualizada correctamente."
        });
    }

    // =========================================================
    // CAMBIAR CONTRASEÑA (Usuario autenticado)
    // =========================================================
    [Authorize]
    [HttpPost("cambiar-password")]
    public async Task<IActionResult>CambiarPassword([FromBody] ChangePasswordDTO dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim);

        var user = await _context.Usuarios.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Usuario no encontrado." });

        // Bloquear si es cuenta de Google (no tiene contraseña)
        if (user.GoogleId != null)
            return BadRequest(new { message = "Las cuentas de Google no pueden cambiar contraseña aquí." });

        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(dto.PasswordActual, user.PasswordHash))
            return BadRequest(new { message = "La contraseña actual es incorrecta." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
        user.DebeCambiarPassword = false; // ← útil para doctores en primer login
        await _context.SaveChangesAsync();

        var newToken = GenerarToken(user);

        return Ok(new 
        { 
            message = "Contraseña actualizada correctamente.",
            token = newToken
        });
    }

    // =========================================================
    // MÉTODOS DE APOYO (Helpers)
    // =========================================================
    
    [Authorize]
    [HttpGet("perfil")]
    public async Task<IActionResult> Perfil()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _context.Usuarios
            .Where(u => u.Email == email)
            .Select(u => new { u.Nombre, u.Apellido, u.Email, u.Rol })
            .FirstOrDefaultAsync();

        return user == null ? NotFound() : Ok(user);
    }

    private string GenerarToken(Usuario user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_key);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol),
            new Claim("MustChangePassword", user.DebeCambiarPassword.ToString().ToLower())
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
    }

    private bool PerfilCompleto(Usuario user)
    {    ///SI EL ADMIN LE FALTA UNOS DATOS SIEMPRE PASA A SU DASHBOARD, Y EVITAR QUE PASE A COMPLETAR EL PERFIL
        if (user.Rol.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase)) return true;
        return !string.IsNullOrEmpty(user.Nombre) && 
               !string.IsNullOrEmpty(user.Apellido) && 
               !string.IsNullOrEmpty(user.Telefono) && 
               !string.IsNullOrEmpty(user.DUI) && // DUI es clave en El Salvador
               !string.IsNullOrEmpty(user.Direccion);
    }

    // =========================================================
    // PARA CREAR DOCTOR (Solo el admin podrá crear nuevas cuentas de doctores)
    // =========================================================
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("create-doctor")]
    public async Task<IActionResult> CreateDoctor([FromBody] RegisterDTO model)
    {
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var emailNormalizado = model.Email.Trim().ToLower();

    if (await _context.Usuarios.AnyAsync(u => u.Email == emailNormalizado))
        return BadRequest(new { message = "El correo ya está registrado." });

    if (!string.IsNullOrEmpty(model.DUI) && await _context.Usuarios.AnyAsync(u => u.DUI == model.DUI))
        return BadRequest(new { message = "El DUI ya está registrado." });

    if (string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest(new
            {
                message = "La contraseña es obligatoria para el doctor."
            });
        }

    var doctor = new Usuario
    {
        Nombre              = model.Nombre,
        Apellido            = model.Apellido,
        Genero              = model.Genero,
        FechaNacimiento     = model.FechaNacimiento,
        Direccion           = model.Direccion,
        Email               = emailNormalizado,
        Telefono            = model.Telefono,
        DUI                 = model.DUI,
        Rol                 = Roles.Doctor,
        DebeCambiarPassword = true,
        EmailVerified       = true                 
    };

    doctor.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

    try
    {
        _context.Usuarios.Add(doctor);
        await _context.SaveChangesAsync();

        return Ok(new {
            id      = doctor.Id,
            message = "Doctor registrado correctamente."
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en CreateDoctor: {ex.Message}");
        return StatusCode(500, new { message = "Error interno al registrar el doctor." });
    }
}
}
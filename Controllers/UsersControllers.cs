using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TelMedAPI.Data;
using TelMedAPI.Helpers;
using TelMedAPI.DTOs;

namespace TelMedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TelMedAPIContext _context;

        public UsersController(TelMedAPIContext context)
        {
            _context = context;
        }

        // ====================================
        // OBTENER PERFIL DEL USUARIO ACTUAL
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            // Usamos NameIdentifier que es el estándar para el ID del usuario
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "El token no contiene un ID de usuario válido." });
            }

            var user = await _context.Usuarios
                .Where(u => u.Id == userId && !u.Eliminado)
                .Select(u => new
                {
                    id = u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.Rol,
                    u.FotoUrl,
                    u.Telefono,
                    u.Direccion,
                    u.Genero,
                    dui = u.DUI,
                    esGoogle = u.GoogleId != null
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { message = "Usuario no encontrado en la base de datos." });

            return Ok(user);
        }

        // ===================================================
        // ACTUALIZAR PERFIL (PACIENTE / DOCTOR / ADMIN)
        [Authorize]
        [HttpPut("perfil")]
        public async Task<IActionResult> UpdatePerfil([FromBody] UpdatePerfilDTO model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var user = await _context.Usuarios.FindAsync(userId);

            if (user == null)
                return NotFound();

            user.Nombre = model.Nombre;
            user.Apellido = model.Apellido;
            user.Direccion = model.Direccion;
            user.Telefono = model.Telefono;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Perfil actualizado correctamente" });
        }

        // ===============================
        // ADMIN - LISTAR USUARIOS
        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.DUI,
                    u.Email,
                    u.Rol,
                    u.Telefono,
                    u.Genero
                })
                .ToListAsync();

            return Ok(users);
        }

        // ===============================
        // DOCTORES
        [Authorize(Roles = Roles.Admin + "," + Roles.Paciente)]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Usuarios
                .Where(u => u.Rol == Roles.Doctor)
                .Select(u => new
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // ===============================
        // PACIENTES
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Usuarios
                .Where(u => u.Rol == Roles.Paciente)
                .Select(u => new
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(patients);
        }

        // ===============================
        // USUARIO POR ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.Rol
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
        }

        // ===================================================
        // Doctores para panel admin (con más detalles)
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("doctors-admin")]
        public async Task<IActionResult> GetDoctorsAdmin()
        {
            var doctors = await _context.Usuarios
                .Where(u => u.Rol == Roles.Doctor && !u.Eliminado)
                .Select(u => new {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.DUI,
                    u.Email,
                    u.Telefono,
                    u.Activo,
                    u.FotoUrl
                })
                .ToListAsync();
            return Ok(doctors);
        }

        // ===================================================
        // ESTADÍSTICAS GENERALES (solo Admin)
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var totalPacientes = await _context.Usuarios
                .CountAsync(u => u.Rol == Roles.Paciente && !u.Eliminado);

            var totalDoctores = await _context.Usuarios
                .CountAsync(u => u.Rol == Roles.Doctor && !u.Eliminado);

            return Ok(new { totalPacientes, totalDoctores });
        }

        // ====================================
        // Cambiar estado activo/inactivo
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDTO dto)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();
            user.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Estado actualizado" });
        }

        // =========================================================
        // Pacientes del doctor (solo los que tienen citas con él)
        [Authorize(Roles = Roles.Doctor)]
        [HttpGet("my-patients")]
        public async Task<IActionResult> GetMyPatients()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var doctorId = int.Parse(userIdClaim);

            var patients = await _context.Citas
                .Where(c => c.DoctorId == doctorId && !c.Paciente.Eliminado)
                .Select(c => c.Paciente)
                .Distinct()
                .Select(u => new {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.DUI,
                    u.Email,
                    u.Telefono,
                    u.Activo,
                    u.FechaNacimiento,
                    u.FotoUrl,

                    //Extrae la fecha de la cita
                    FechaUltimaConsulta = _context.Consultas
                    .Where(con => con.Cita.PacienteId == u.Id)
                    .OrderByDescending(con => con.Fecha)
                    .Select(con => con.Fecha)
                    .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(patients);
        }

        // =========================================================
        // Pacientes para panel admin (con detalle completo)
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("patients-admin")]
        public async Task<IActionResult> GetPatientsAdmin()
        {
            var patients = await _context.Usuarios
                .Where(u => u.Rol == Roles.Paciente && !u.Eliminado)
                .Select(u => new {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.DUI,
                    u.Email,
                    u.Telefono,
                    u.Genero,
                    u.Activo
                })
                .ToListAsync();
            return Ok(patients);
        }
    }
}
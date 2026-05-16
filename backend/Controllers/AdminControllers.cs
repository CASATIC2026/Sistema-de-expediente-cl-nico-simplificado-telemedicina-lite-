using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TelMedAPI.Data;
using TelMedAPI.Helpers;
using TelMedAPI.Models;
using TelMedAPI.DTOs;

namespace TelMedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly TelMedAPIContext _context;

        public AdminController(TelMedAPIContext context)
        {
            _context = context;
        }

        // ============================
        // Pacientes
        [HttpGet("pacientes")]
        public async Task<IActionResult> GetPacientes()
        {
            var pacientes = await _context.Usuarios
                .Where(u => u.Rol == Roles.Paciente && !u.Eliminado)
                .ToListAsync();

            return Ok(pacientes);
        }

        [HttpDelete("pacientes/{id}")]
        public async Task<IActionResult> EliminarPaciente(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            
            if (usuario == null || usuario.Rol != Roles.Paciente)
                return NotFound();

            //Protección Admin
            if (usuario.Rol == Roles.Admin)
                return BadRequest("No puedes eliminar un administrador");

            usuario.Eliminado = true;

            await _context.SaveChangesAsync();

            return Ok("Paciente eliminado (soft delete)");
        }

        // ============================
        // Doctores

        [HttpGet("doctores")]
        public async Task<IActionResult> GetDoctores()
        {
            var doctores = await _context.Usuarios
                .Where(u => u.Rol == Roles.Doctor && !u.Eliminado)
                .ToListAsync();

            return Ok(doctores);
        }

        

        [HttpDelete("doctores/{id}")]
        public async Task<IActionResult> EliminarDoctor(int id)
        {
            var doctor = await _context.Usuarios.FindAsync(id);

            if (doctor == null || doctor.Rol != Roles.Doctor)
                return NotFound();

            //Protección Admin
            if (doctor.Rol == Roles.Admin)
                return BadRequest("No puedes eliminar un administrador");

            doctor.Eliminado = true;

            await _context.SaveChangesAsync();

            return Ok("Doctor eliminado");
        }

        // ============================
        // Citas
        [HttpGet("citas")]
        public async Task<IActionResult> GetTodasCitas()
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .ToListAsync();

            return Ok(citas);
        }

        // ============================
        // Buscar Paciente por DUI
        [HttpGet("buscar-paciente/{dui}")]
        public async Task<IActionResult> BuscarPacientePorDui(string dui)
        {
            var paciente = await _context.Usuarios
                .Where(u => u.DUI == dui && u.Rol == Roles.Paciente && !u.Eliminado)
                .Select(u => new { 
                    u.Id, 
                    u.Nombre, 
                    u.Apellido,
                    u.DUI
                })
                .FirstOrDefaultAsync();

            if (paciente == null) 
                return NotFound("No se encontró ningún paciente con ese DUI");

            return Ok(paciente);
        }

        // ============================
        // HORARIOS DE ATENCIÓN
        [HttpGet("horarios")]
        public async Task<IActionResult> GetHorarios()
        {
            var horarios = await _context.HorariosDoctor
                .OrderBy(h => h.DiaSemana)
                .ToListAsync();

            return Ok(horarios);
        }

        [HttpPut("horarios")]
        public async Task<IActionResult> ActualizarHorarios(
            [FromBody] List<UpdateHorarioDTO> horariosDto)
        {
            if (horariosDto == null || !horariosDto.Any())
            {
                return BadRequest(new
                {
                    message = "Debes enviar al menos un horario."
                });
            }

            foreach (var item in horariosDto)
            {
                var horario = await _context.HorariosDoctor
                    .FirstOrDefaultAsync(h => h.DiaSemana == item.DiaSemana);

                if (horario == null)
                    continue;

                if (item.HoraInicio >= item.HoraFin)
                {
                    return BadRequest(new
                    {
                        message = $"La hora de inicio debe ser menor que la hora final para el día {item.DiaSemana}."
                    });
                }

                horario.Activo = item.Activo;
                horario.HoraInicio = item.HoraInicio;
                horario.HoraFin = item.HoraFin;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Horarios actualizados correctamente."
            });
        }
    }
}
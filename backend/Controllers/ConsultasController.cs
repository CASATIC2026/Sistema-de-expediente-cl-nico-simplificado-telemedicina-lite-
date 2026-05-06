using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TelMedAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using TelMedAPI.Data;
using TelMedAPI.DTOs;
using TelMedAPI.Models;
using TelMedAPI.Services;
using QuestPDF.Fluent;

namespace TelMedAPI.Controllers
{
    // Controlador para manejar consultas médicas
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultasController : ControllerBase
    {
        private readonly TelMedAPIContext _context;

        public ConsultasController(TelMedAPIContext context)
        {
            _context = context;
        }

        [Authorize(Roles = Roles.Admin + "," + Roles.Doctor)]
        [HttpGet("paciente/{pacienteId}")]
        public async Task<IActionResult> GetHistorial(int pacienteId)
        {
            var consultas = await _context.Consultas
                .Include(c => c.Cita)
                .ThenInclude(c => c.Doctor)
                .Where(c => c.Cita != null && c.Cita.PacienteId == pacienteId)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return Ok(consultas);
        }

        // Crear consulta para cita
        [Authorize(Roles = Roles.Doctor)]
        [HttpPost]
        public async Task<IActionResult> CrearConsulta(CreateConsultaDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rol != Roles.Doctor)
                return Forbid();

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.IdCita == dto.CitaId);

            if (cita == null)
                return NotFound("Cita no encontrada");

            // Validar que el doctor sea dueño
            if (cita.DoctorId != userId)
                return Forbid("No puedes atender esta cita");

            // Evitar duplicados
            var existe = await _context.Consultas
                .AnyAsync(c => c.CitaId == dto.CitaId);

            if (existe)
                return BadRequest("Esta cita ya tiene una consulta registrada");

            var consulta = new Consulta
            {
                CitaId = dto.CitaId,
                Fecha = DateTime.UtcNow,
                Sintomas = dto.Sintomas,
                Evolucion = dto.Evolucion,
                Diagnostico = dto.Diagnostico,
                Tratamiento = dto.Tratamiento,
                Observaciones = dto.Observaciones,
                MedicamentosJson = dto.MedicamentosJson
            };

            _context.Consultas.Add(consulta);

            // Cambiar estado automáticamente
            cita.Estado = CitaEstados.Finalizada;

            await _context.SaveChangesAsync();

            return Ok(consulta);
        }

        // Descargar PDF de consulta
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                .ThenInclude(c => c.Paciente)
                .Include(c => c.Cita.Doctor)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
                return NotFound();

            var document = new CitaReport(consulta.Cita, consulta);
            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf", $"Consulta_{id}.pdf");
        }
    }
}
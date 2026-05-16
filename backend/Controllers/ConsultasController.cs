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
        private readonly EmailService _emailService;

        public ConsultasController(TelMedAPIContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
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

            if (dto.TieneIncapacidad)
            {
                if (!dto.FechaInicioIncapacidad.HasValue || !dto.FechaFinIncapacidad.HasValue)
                    return BadRequest("Si se genera incapacidad, debe incluir fecha de inicio y fecha de fin.");

                var fechaInicioUtc = DateTime.SpecifyKind(dto.FechaInicioIncapacidad.Value, DateTimeKind.Utc);
                var fechaFinUtc = DateTime.SpecifyKind(dto.FechaFinIncapacidad.Value, DateTimeKind.Utc);

                if (fechaFinUtc.Date < fechaInicioUtc.Date)
                    return BadRequest("La fecha fin de la incapacidad no puede ser anterior a la fecha inicio.");

                if (string.IsNullOrWhiteSpace(dto.MotivoIncapacidad))
                    return BadRequest("El motivo de incapacidad es requerido cuando se emite incapacidad.");

                if (!dto.DiasIncapacidad.HasValue)
                {
                    dto.DiasIncapacidad = (int)(fechaFinUtc.Date - fechaInicioUtc.Date).TotalDays + 1;
                }

                dto.FechaInicioIncapacidad = fechaInicioUtc;
                dto.FechaFinIncapacidad = fechaFinUtc;
            }
            else
            {
                dto.FechaInicioIncapacidad = null;
                dto.FechaFinIncapacidad = null;
                dto.DiasIncapacidad = null;
                dto.MotivoIncapacidad = string.Empty;
                dto.ObservacionesIncapacidad = string.Empty;
            }

            var consulta = new Consulta
            {
                CitaId = dto.CitaId,
                Fecha = DateTime.UtcNow,
                Sintomas = dto.Sintomas,
                Evolucion = dto.Evolucion,
                Diagnostico = dto.Diagnostico,
                Tratamiento = dto.Tratamiento,
                Observaciones = dto.Observaciones,
                MedicamentosJson = dto.MedicamentosJson,
                TieneIncapacidad = dto.TieneIncapacidad,
                FechaInicioIncapacidad = dto.FechaInicioIncapacidad,
                FechaFinIncapacidad = dto.FechaFinIncapacidad,
                DiasIncapacidad = dto.DiasIncapacidad,
                MotivoIncapacidad = dto.MotivoIncapacidad,
                ObservacionesIncapacidad = dto.ObservacionesIncapacidad
            };

            _context.Consultas.Add(consulta);

            // Cambiar estado automáticamente
            cita.Estado = CitaEstados.Finalizada;

            await _context.SaveChangesAsync();

            if (consulta.TieneIncapacidad && cita.Paciente != null && !string.IsNullOrWhiteSpace(cita.Paciente.Email))
            {
                try
                {
                    var document = new CitaReport(cita, consulta);
                    var pdf = document.GeneratePdf();
                    var doctorNombre = cita.Doctor == null
                        ? ""
                        : $"{cita.Doctor.Nombre} {cita.Doctor.Apellido}".Trim();

                    await _emailService.EnviarCorreoIncapacidad(
                        cita.Paciente.Email,
                        cita.Paciente.Nombre,
                        doctorNombre,
                        consulta.FechaInicioIncapacidad!.Value,
                        consulta.FechaFinIncapacidad!.Value,
                        consulta.DiasIncapacidad ?? (int)(consulta.FechaFinIncapacidad.Value.Date - consulta.FechaInicioIncapacidad.Value.Date).TotalDays + 1,
                        consulta.MotivoIncapacidad,
                        consulta.ObservacionesIncapacidad,
                        pdf,
                        $"Incapacidad_{consulta.IdConsulta}.pdf"
                    );
                }
                catch
                {
                    // El envío de correo no debe bloquear la respuesta principal.
                }
            }

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
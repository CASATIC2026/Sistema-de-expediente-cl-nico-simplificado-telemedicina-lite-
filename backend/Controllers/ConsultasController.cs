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

            if (cita.Estado != CitaEstados.EnConsulta)
            {
                return BadRequest("La cita debe estar en consulta para registrar el expediente médico.");
            }

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
                ObservacionesIncapacidad = dto.ObservacionesIncapacidad,

                TieneOrdenLaboratorio = dto.TieneOrdenLaboratorio,
                ExamenesJson = dto.TieneOrdenLaboratorio ? (dto.ExamenesJson ?? string.Empty) : string.Empty,
                EstadoOrden = dto.TieneOrdenLaboratorio ? "Pendiente" : string.Empty,
                ObservacionesLaboratorio = string.Empty,
                ResultadoPdfPath = null
            };

            _context.Consultas.Add(consulta);

            await _context.SaveChangesAsync();

            if (consulta.TieneIncapacidad && cita.Paciente != null && !string.IsNullOrWhiteSpace(cita.Paciente.Email))
            {
                try
                {
                    var document = new IncapacidadReport(cita, consulta);
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


        // Descargar PDF de orden de laboratorio
        [HttpGet("{id}/orden-pdf")]
        public async Task<IActionResult> DescargarOrdenPdf(int id)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                .ThenInclude(c => c.Paciente)
                .Include(c => c.Cita.Doctor)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
                return NotFound("Consulta no encontrada");

            if (!consulta.TieneOrdenLaboratorio)
                return BadRequest("Esta consulta no tiene orden de laboratorio");

            var document = new OrdenLaboratorioReport(consulta.Cita, consulta);
            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf", $"OrdenLaboratorio_{id}.pdf");
        }

        // Paciente sube PDF de resultados
        [Authorize(Roles = Roles.Paciente)]
        [HttpPost("{id}/subir-resultado")]
        public async Task<IActionResult> SubirResultado(int id, IFormFile archivo)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
                return NotFound("Consulta no encontrada");

            if (!consulta.TieneOrdenLaboratorio)
                return BadRequest("Esta consulta no tiene orden de laboratorio");

            if (consulta.EstadoOrden == "Revisado")
                return BadRequest("El doctor ya revisó esta orden");

            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debes adjuntar un archivo PDF");

            if (archivo.ContentType != "application/pdf")
                return BadRequest("Solo se aceptan archivos PDF");

            if (archivo.Length > 5 * 1024 * 1024)
                return BadRequest("El archivo no puede superar los 5MB");

            // Guardar el archivo en disco
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Resultados");
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"Resultado_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            var rutaCompleta  = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Guardar solo la ruta relativa, no la absoluta
            consulta.ResultadoPdfPath = Path.Combine("Uploads", "Resultados", nombreArchivo);
            consulta.EstadoOrden      = "ResultadosSubidos";

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Resultados subidos correctamente" });
        }

        // Doctor revisa resultados y envía observaciones
        [Authorize(Roles = Roles.Doctor)]
        [HttpPost("{id}/revisar-laboratorio")]
        public async Task<IActionResult> RevisarLaboratorio(int id, [FromBody] RevisarLaboratorioDTO dto)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                .ThenInclude(c => c.Paciente)
                .Include(c => c.Cita.Doctor)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
                return NotFound("Consulta no encontrada");

            if (consulta.EstadoOrden != "ResultadosSubidos")
                return BadRequest("El paciente aún no ha subido sus resultados");

            if (string.IsNullOrWhiteSpace(dto.Observaciones))
                return BadRequest("Debes ingresar tus observaciones");

            consulta.ObservacionesLaboratorio = dto.Observaciones;
            consulta.EstadoOrden              = "Revisado";

            await _context.SaveChangesAsync();

            // Enviar correo al paciente
            if (consulta.Cita?.Paciente?.Email != null)
            {
                try
                {
                    var doctorNombre = $"{consulta.Cita.Doctor?.Nombre} {consulta.Cita.Doctor?.Apellido}".Trim();
                    await _emailService.EnviarNotificacionRevisionLaboratorio(
                        consulta.Cita.Paciente.Email,
                        consulta.Cita.Paciente.Nombre,
                        doctorNombre
                    );
                }
                catch
                {
                    // El correo no debe bloquear la respuesta
                }
            }

            return Ok(new { mensaje = "Revisión enviada correctamente" });
        }

        // Doctor descarga el PDF de resultados que subió el paciente
        [Authorize(Roles = Roles.Doctor + "," + Roles.Admin)]
        [HttpGet("{id}/resultado-pdf")]
        public async Task<IActionResult> DescargarResultadoPdf(int id)
        {
            var consulta = await _context.Consultas
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(consulta.ResultadoPdfPath))
                return NotFound("El paciente aún no ha subido resultados");

            var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), consulta.ResultadoPdfPath);

            if (!System.IO.File.Exists(rutaAbsoluta))
                return NotFound("Archivo no encontrado en el servidor");

            var bytes = await System.IO.File.ReadAllBytesAsync(rutaAbsoluta);
            return File(bytes, "application/pdf", $"Resultados_{id}.pdf");
        }


    }
}
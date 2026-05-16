using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelMedAPI.Data;
using FluentValidation;
using TelMedAPI.DTOs;
using TelMedAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TelMedAPI.Helpers;
using TelMedAPI.Services;
using QuestPDF.Fluent;

namespace TelMedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly TelMedAPIContext _context;
        private readonly IValidator<CreateCitaDTO> _validator;
        private readonly EmailService _emailService;

        public CitasController(
            TelMedAPIContext context,
            IValidator<CreateCitaDTO> validator,
            EmailService emailService)
        {
            _context = context;
            _validator = validator;
            _emailService = emailService;
        }

        // =========================================================
        // DASHBOARD
        [Authorize(Roles = Roles.Admin + "," + Roles.Doctor)]
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == null) return Unauthorized();

            var query = _context.Citas.AsQueryable();

            if (rol == Roles.Doctor)
                query = query.Where(c => c.DoctorId == userId);

            var zonaSV    = GetZonaElSalvador();
            var ahoraSV   = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaSV);
            var hoySV     = ahoraSV.Date;
            var hoyInicio = TimeZoneInfo.ConvertTimeToUtc(hoySV, zonaSV);
            var hoyFin    = TimeZoneInfo.ConvertTimeToUtc(hoySV.AddDays(1), zonaSV);

            
            var queryHoy = query.Where(c => c.FechaInicio >= hoyInicio && c.FechaInicio < hoyFin);

            var resumen = new
            {
                Pendientes  = await queryHoy.CountAsync(c => c.Estado == CitaEstados.Pendiente),
                Confirmadas = await queryHoy.CountAsync(c => c.Estado == CitaEstados.Confirmada),
                Canceladas  = await queryHoy.CountAsync(c => c.Estado == CitaEstados.Cancelada),
                EnConsulta  = await queryHoy.CountAsync(c => c.Estado == CitaEstados.EnConsulta),
                Finalizadas = await queryHoy.CountAsync(c => c.Estado == CitaEstados.Finalizada),
                Hoy         = await queryHoy.CountAsync()
            };

            return Ok(resumen);
        }
        // =========================================================
        // LISTAR CITAS
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCitas()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == null) return Unauthorized();

            var query = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .AsQueryable();

            if (rol == Roles.Doctor)
                query = query.Where(c => c.DoctorId == userId);
            else if (rol == Roles.Paciente)
                query = query.Where(c => c.PacienteId == userId);

            var citas = await query
                .OrderBy(c => c.FechaInicio)
                .Select(c => new CitaCalendarDto
                {
                    IdCita                 = c.IdCita,
                    Titulo                 = c.Motivo,
                    Start                  = c.FechaInicio,
                    End                    = c.FechaFin,
                    Estado                 = c.Estado,
                    PacienteNombreCompleto = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                    TipoConsulta           = c.TipoConsulta,
                    PacienteId             = c.PacienteId,
                    LinkReunion            = c.LinkReunion,
                    DuiPaciente            = c.Paciente.DUI,
                    TelefonoPaciente       = c.Paciente.Telefono,

                    DoctorNombreCompleto   = c.Doctor != null  
                    ? c.Doctor.Nombre + " " + c.Doctor.Apellido
                    : "Sin asignar"


                })
                .ToListAsync();

            return Ok(citas);
        }

        // =========================================================
        // BUSCAR PACIENTE POR DUI (solo Admin)
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("buscar-dui")]
        public async Task<IActionResult> BuscarPorDui([FromQuery] string dui)
        {
            if (string.IsNullOrWhiteSpace(dui))
                return BadRequest(new { message = "El DUI es requerido." });

            var paciente = await _context.Usuarios
                .Where(u => u.DUI == dui
                         && u.Rol == Roles.Paciente
                         && !u.Eliminado)
                .Select(u => new { u.Id, u.Nombre, u.Apellido, u.Email })
                .FirstOrDefaultAsync();

            if (paciente == null)
                return NotFound(new { message = "No se encontró ningún paciente con ese DUI." });

            return Ok(paciente);
        }

        // =========================================================
        // CITA POR ID
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCitaById(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Where(c => c.IdCita == id)
                .Select(c => new CitaCalendarDto
                {
                    IdCita                 = c.IdCita,
                    Titulo                 = c.Motivo,
                    Start                  = c.FechaInicio,
                    End                    = c.FechaFin,
                    Estado                 = c.Estado,
                    PacienteNombreCompleto = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                    TipoConsulta           = c.TipoConsulta,
                    PacienteId             = c.PacienteId,
                    LinkReunion            = c.LinkReunion,
                    DuiPaciente            = c.Paciente.DUI,
                    TelefonoPaciente       = c.Paciente.Telefono,
                    CorreoPaciente          = c.Paciente.Email,
                    FechaNacimientoPaciente = c.Paciente.FechaNacimiento.ToString(),
                    FotoPaciente            = c.Paciente.FotoUrl
                })
                .FirstOrDefaultAsync();

            if (cita == null)
                return NotFound(new { message = "Cita no encontrada." });

            return Ok(cita);
        }

        // =========================================================
        // VER CITAS PARA ROL PACIENTE
        [Authorize(Roles = Roles.Paciente)]
        [HttpGet("mis-citas")]
        public async Task<IActionResult> GetMisCitas()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var citas = await _context.Citas
                .Include(c => c.Doctor)
                .Include(c => c.Paciente)
                .Where(c => c.Paciente.Email == email)
                .Select(c => new CitaCalendarDto
                {
                    PacienteNombreCompleto = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                    PacienteId             = c.PacienteId,
                    IdCita                 = c.IdCita,
                    Titulo                 = c.Motivo,
                    Start                  = c.FechaInicio,
                    End                    = c.FechaFin,
                    Estado                 = c.Estado,
                    TipoConsulta           = c.TipoConsulta,
                    LinkReunion            = c.LinkReunion,       
                    
                })
                .ToListAsync();

            return Ok(citas);
        }

        // =========================================================
        // HISTORIAL COMPLETO PACIENTE
        [Authorize(Roles = Roles.Paciente)]
        [HttpGet("mis-citas-detalle")]
        public async Task<IActionResult> GetMisCitasDetalle()
        {
            // ✅ FIX: Null-safe claim parsing
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var citas = await _context.Citas
                .Include(c => c.Doctor)
                .Include(c => c.Consulta)
                .Where(c => c.PacienteId == userId)
                .OrderByDescending(c => c.FechaInicio)
                .Select(c => new
                {
                    c.IdCita,
                    c.Motivo,
                    c.TipoConsulta,
                    c.Estado,
                    c.FechaInicio,
                    c.FechaFin,
                    Doctor = c.Doctor != null
                        ? c.Doctor.Nombre + " " + c.Doctor.Apellido
                        : "Sin asignar",
                    c.LinkReunion,
                    Consulta = c.Consulta == null ? null : new
                    {
                        c.Consulta.Sintomas,
                        c.Consulta.Diagnostico,
                        c.Consulta.Tratamiento,
                        c.Consulta.Observaciones,
                        c.Consulta.MedicamentosJson,
                        c.Consulta.Fecha
                    }
                })
                .ToListAsync();

            return Ok(citas);
        }

        // =========================================================
        // DOCTOR Y ADMIN TIENEN ACCESO AL HISTORIAL DEL PACIENTE(admin con ciertas restricciones)
        // =========================================================
        [Authorize(Roles = Roles.Doctor + "," + Roles.Admin)]
        [HttpGet("historial-paciente/{pacienteId}")]
        public async Task<IActionResult> GetHistorialPaciente(int pacienteId)
        {
            var citas = await _context.Citas
                .Include(c => c.Doctor)
                .Include(c => c.Consulta)
                .Where(c => c.PacienteId == pacienteId)
                .OrderByDescending(c => c.FechaInicio)
                .Select(c => new {
                    idCita       = c.IdCita,
                    motivo       = c.Motivo,
                    tipoConsulta = c.TipoConsulta,
                    estado       = c.Estado,
                    fechaInicio  = c.FechaInicio,
                    doctor       = c.Doctor != null
                                ? c.Doctor.Nombre + " " + c.Doctor.Apellido
                                : "Sin asignar",
                    consulta     = c.Consulta == null ? null : new {
                        c.Consulta.Diagnostico,
                        c.Consulta.Sintomas,
                        c.Consulta.Tratamiento,
                        c.Consulta.Observaciones
                    }
                })
                .ToListAsync();

            return Ok(citas);
        }

        // =========================================================
        // RECETA MÉDICA EN PDF
        // =========================================================
        // Después
        [Authorize(Roles = Roles.Paciente + "," + Roles.Doctor + "," + Roles.Admin)]
        [HttpGet("{id}/receta")]
        public async Task<IActionResult> DescargarReceta(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);
            var rol    = User.FindFirst(ClaimTypes.Role)?.Value;

            // Paciente solo puede ver su propia receta
            // Doctor y admin pueden ver cualquiera
            var cita = rol == Roles.Paciente
                ? await _context.Citas
                    .Include(c => c.Paciente)
                    .Include(c => c.Doctor)
                    .Include(c => c.Consulta)
                    .FirstOrDefaultAsync(c => c.IdCita == id && c.PacienteId == userId)
                : await _context.Citas
                    .Include(c => c.Paciente)
                    .Include(c => c.Doctor)
                    .Include(c => c.Consulta)
                    .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null)
                return NotFound(new { message = "Cita no encontrada." });

            if (cita.Consulta == null)
                return BadRequest(new { message = "Esta cita no tiene una consulta registrada." });

            var document = new CitaReport(cita, cita.Consulta);
            var pdf      = document.GeneratePdf();

            return File(pdf, "application/pdf",
            $"RecetaMedica_{cita.Paciente?.Nombre}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // =========================================================
        // Horarios disponibles para agendar citas
       // =========================================================
// GET horario de un doctor específico (Admin + Doctor)
// GET /api/citas/horarios-disponibles?doctorId=1
[Authorize]
[HttpGet("horarios-disponibles")]
public async Task<IActionResult> GetHorariosDisponibles([FromQuery] int doctorId)
{
    if (doctorId == 0)
        return BadRequest(new { message = "doctorId es requerido." });

    var horarios = await _context.HorariosDoctor
        .Where(h => h.DoctorId == doctorId)
        .OrderBy(h => h.DiaSemana)
        .Select(h => new
        {
            h.DiaSemana,
            h.Activo,
            HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
            HoraFin    = h.HoraFin.ToString(@"hh\:mm")
        })
        .ToListAsync();

    return Ok(horarios);
}

// =========================================================
// PUT actualizar horario completo de un doctor (solo Admin)
// PUT /api/citas/horarios-doctor/1
[Authorize(Roles = Roles.Admin)]
[HttpPut("horarios-doctor/{doctorId}")]
public async Task<IActionResult> ActualizarHorarioDoctor(
    int doctorId,
    [FromBody] List<HorarioDoctorDTO> horarios)
{
    // Verificar que el doctor existe
    var doctorExiste = await _context.Usuarios
        .AnyAsync(u => u.Id == doctorId && u.Rol == Roles.Doctor);

    if (!doctorExiste)
        return NotFound(new { message = "Doctor no encontrado." });

    // Borrar horarios anteriores de ese doctor y reemplazar
    var horariosExistentes = await _context.HorariosDoctor
        .Where(h => h.DoctorId == doctorId)
        .ToListAsync();

    _context.HorariosDoctor.RemoveRange(horariosExistentes);

    foreach (var h in horarios)
    {
        _context.HorariosDoctor.Add(new HorarioDoctor
        {
            DoctorId   = doctorId,
            DiaSemana  = h.DiaSemana,
            HoraInicio = TimeSpan.Parse(h.HoraInicio),
            HoraFin    = TimeSpan.Parse(h.HoraFin),
            Activo     = h.Activo
        });
    }

    await _context.SaveChangesAsync();
    return Ok(new { message = "Horario actualizado correctamente." });
}

// =========================================================
// GET slots disponibles para agendar (paciente/admin al crear cita)
// GET /api/citas/slots-disponibles?doctorId=1&fecha=2026-05-10
[Authorize]
[HttpGet("slots-disponibles")]
public async Task<IActionResult> GetSlotsDisponibles(
    [FromQuery] int    doctorId,
    [FromQuery] string fecha)
{
    if (!DateTime.TryParse(fecha, out var fechaDate))
        return BadRequest(new { message = "Fecha inválida." });

    // 1. Día de la semana de la fecha solicitada
    var diaSemana = (int)fechaDate.DayOfWeek;

    // 2. Buscar el horario del doctor para ese día
    var horario = await _context.HorariosDoctor
        .FirstOrDefaultAsync(h => h.DoctorId == doctorId
                               && h.DiaSemana == diaSemana
                               && h.Activo);

    // Si no hay horario activo ese día → lista vacía
    if (horario == null)
        return Ok(new List<object>());

    // 3. Citas ya ocupadas ese día para ese doctor (en UTC)
    var diaInicioUtc = DateTime.SpecifyKind(fechaDate.Date, DateTimeKind.Utc);
    var diaFinUtc    = diaInicioUtc.AddDays(1);

    var horasOcupadas = await _context.Citas
        .Where(c =>
            c.DoctorId    == doctorId        &&
            c.Estado      != CitaEstados.Cancelada &&
            c.FechaInicio >= diaInicioUtc    &&
            c.FechaInicio  < diaFinUtc)
        .Select(c => c.FechaInicio)
        .ToListAsync();

    // 4. Generar slots de 30 min entre HoraInicio y HoraFin
    const int duracion = 30;
    var slots  = new List<object>();
    var cursor = horario.HoraInicio;

    while (cursor.Add(TimeSpan.FromMinutes(duracion)) <= horario.HoraFin)
    {
        // Convertir slot local SV (UTC-6) a UTC para comparar con la BD
        var slotUtc = diaInicioUtc.Add(cursor).AddHours(6);

        var ocupado = horasOcupadas.Any(h =>
            h.Hour   == slotUtc.Hour &&
            h.Minute == slotUtc.Minute);

        var inicio  = DateTime.Today.Add(cursor);
        var fin     = inicio.AddMinutes(duracion);
        var periodo = cursor.Hours < 12 ? "AM" : "PM";

        slots.Add(new
        {
            valor    = cursor.ToString(@"hh\:mm"),
            etiqueta = $"{inicio:hh:mm} - {fin:hh:mm} {periodo}",
            ocupado
        });

        cursor = cursor.Add(TimeSpan.FromMinutes(duracion));
    }

    return Ok(slots);
}

        // =========================================================
        // CALENDARIO
        [Authorize]
        [HttpGet("calendario")]
        public async Task<ActionResult<IEnumerable<CitaCalendarDto>>> GetCitasCalendario(
            [FromQuery] int?      doctorId     = null,
            [FromQuery] DateTime? fecha        = null,
            [FromQuery] bool      soloOcupados = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol         = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null || rol == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var query = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .Where(c => !c.Paciente.Eliminado)
                .AsQueryable();

            if (soloOcupados)
                query = query.Where(c => c.Estado != CitaEstados.Cancelada);

            if (doctorId.HasValue)
                query = query.Where(c => c.DoctorId == doctorId.Value);

            if (fecha.HasValue)
            {
                var fechaInicio = new DateTimeOffset(fecha.Value.Date, TimeSpan.Zero);
                var fechaFin    = fechaInicio.AddDays(1);
                query = query.Where(c => c.FechaInicio >= fechaInicio && c.FechaInicio < fechaFin);
            }
            else
            {
                if (rol == Roles.Paciente)
                    query = query.Where(c => c.PacienteId == userId);
                else if (rol == Roles.Doctor)
                    query = query.Where(c => c.DoctorId == userId);
            }

            var citas = await query
                .OrderBy(c => c.FechaInicio)
                .Select(c => new CitaCalendarDto
                {
                    IdCita                 = c.IdCita,
                    Titulo                 = c.Motivo,
                    Start                  = c.FechaInicio,
                    End                    = c.FechaFin,
                    Estado                 = c.Estado,
                    PacienteNombreCompleto = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                    TipoConsulta           = c.TipoConsulta,
                    PacienteId             = c.PacienteId,
                    LinkReunion            = c.LinkReunion
                })
                .ToListAsync();

            return Ok(citas);
        }

        // =========================================================
        // CREAR CITA
        // =========================================================
        // CREAR CITA
        [Authorize(Roles = Roles.Paciente + "," + Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CrearCita([FromBody] CreateCitaDTO dto)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var fechaInicioUtc   = dto.FechaInicio.UtcDateTime;
            var fechaFinUtc      = dto.FechaFin.UtcDateTime;

            // ✅ Convertir a hora local SV para comparar contra horario del doctor
            var fechaInicioLocal = TimeZoneInfo.ConvertTimeFromUtc(fechaInicioUtc, GetZonaElSalvador());
            var horaCita         = fechaInicioLocal.TimeOfDay;

            // Validar que inicio sea antes que fin
            if (fechaInicioUtc >= fechaFinUtc)
                return BadRequest(new { message = "La fecha de inicio debe ser anterior a la fecha de fin." });

            // Validar que la cita no sea en el pasado
            if (fechaInicioUtc < DateTime.UtcNow)
                return BadRequest(new { message = "No puedes agendar una cita en el pasado." });

            // Día de la semana en hora local SV
            var diaSemana = (int)fechaInicioLocal.DayOfWeek;

            // ✅ Buscar horario del doctor específico para ese día
            var horarioDisponible = await _context.HorariosDoctor
                .FirstOrDefaultAsync(h => h.DiaSemana == diaSemana
                                    && h.DoctorId  == dto.DoctorId
                                    && h.Activo);

            if (horarioDisponible == null)
                return BadRequest(new { message = "El doctor no tiene horario disponible para ese día." });

            if (horaCita < horarioDisponible.HoraInicio || horaCita >= horarioDisponible.HoraFin)
                return BadRequest(new { message = $"El doctor atiende de {horarioDisponible.HoraInicio:hh\\:mm} a {horarioDisponible.HoraFin:hh\\:mm} ese día." });

            // Validar conflicto de horario (anti-solapamiento)
            var existeChoque = await _context.Citas.AnyAsync(c =>
                c.DoctorId   == dto.DoctorId &&
                c.Estado     != CitaEstados.Cancelada &&
                c.FechaInicio < fechaFinUtc &&
                c.FechaFin    > fechaInicioUtc);

            if (existeChoque)
                return BadRequest(new { message = "El doctor ya tiene una cita agendada en ese horario." });

            // Determinar el ID del paciente según el rol
            int finalPacienteId;
            if (User.IsInRole(Roles.Admin))
            {
                finalPacienteId = dto.PacienteId;
            }
            else
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();
                finalPacienteId = int.Parse(userIdClaim);
            }

             //Validar que el paciente esté activo
            var paciente = await _context.Usuarios.FindAsync(finalPacienteId);
            if (paciente == null || !paciente.Activo)
                return BadRequest(new { message = "El paciente está inactivo y no puede agendar citas." });



            var identificadorUnico = Guid.NewGuid().ToString().Substring(0, 8);
            var linkCalculado      = $"https://meet.jit.si/NovoMed{identificadorUnico}#config.prejoinPageEnabled=false";

            var cita = new Cita
            {
                Motivo       = dto.Motivo,
                FechaInicio  = fechaInicioUtc,
                FechaFin     = fechaFinUtc,
                TipoConsulta = dto.TipoConsulta,
                PacienteId   = finalPacienteId,
                DoctorId     = dto.DoctorId,
                Estado       = CitaEstados.Pendiente,
                LinkReunion  = linkCalculado
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return Ok(cita);
        }


        // =========================================================
        // INICIAR CONSULTA
        [Authorize(Roles = Roles.Doctor)]
        [HttpPut("{id}/iniciar")]
        public async Task<IActionResult> IniciarConsulta(int id)
        {
            // ✅ FIX: Null-safe claim parsing
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
                return NotFound(new { message = "Cita no encontrada." });

            if (cita.DoctorId != userId)
                return Forbid();

            if (cita.Estado == CitaEstados.Cancelada || cita.Estado == CitaEstados.Finalizada)
                return BadRequest(new { message = "No puedes iniciar esta cita." });

            cita.Estado = CitaEstados.EnConsulta;
            await _context.SaveChangesAsync();

            return Ok(cita);
        }

        // =========================================================
        // FINALIZAR CONSULTA
        [Authorize(Roles = Roles.Doctor)]
        [HttpPut("{id}/finalizar")]
        public async Task<IActionResult> FinalizarConsulta(int id)
        {
            // ✅ FIX: Null-safe claim parsing
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
                return NotFound(new { message = "Cita no encontrada." });

            if (cita.DoctorId != userId)
                return Forbid();

            if (cita.Estado != CitaEstados.EnConsulta)
                return BadRequest(new { message = "La cita no está en consulta." });

            cita.Estado = CitaEstados.Finalizada;
            await _context.SaveChangesAsync();

            return Ok(cita);
        }

        // =========================================================
        // CANCELAR CITA
        [Authorize(Roles = Roles.Paciente + "," + Roles.Admin)]
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null)
                return NotFound(new { message = "Cita no encontrada." });

            if (rol == Roles.Paciente && cita.PacienteId != userId)
                return Forbid();

            if (cita.Estado == CitaEstados.Finalizada)
                return BadRequest(new { message = "No puedes cancelar una cita ya finalizada." });

            if (cita.Estado == CitaEstados.Cancelada)
                return BadRequest(new { message = "Esta cita ya fue cancelada." });

            if (rol == Roles.Paciente)
            {
                var tiempoRestante = cita.FechaInicio.UtcDateTime - DateTime.UtcNow;
                if (tiempoRestante < TimeSpan.FromHours(24))
                {
                    return BadRequest(new
                    {
                        message = "No puedes cancelar esta cita con menos de 24 horas de anticipación."
                    });
                }
            }

            cita.Estado = CitaEstados.Cancelada;
            await _context.SaveChangesAsync();

            try
            {
                if (cita.Paciente != null && !string.IsNullOrWhiteSpace(cita.Paciente.Email))
                {
                    await _emailService.EnviarNotificacionCancelacionCita(
                        cita.Paciente.Email,
                        cita.Paciente.Nombre + " " + cita.Paciente.Apellido,
                        cita.Doctor != null
                            ? cita.Doctor.Nombre + " " + cita.Doctor.Apellido
                            : "Doctor no asignado",
                        cita.FechaInicio,
                        cita.Motivo,
                        cita.TipoConsulta);
                }

                await _emailService.EnviarNotificacionCancelacionAdmin(
                    cita.Paciente != null
                        ? cita.Paciente.Nombre + " " + cita.Paciente.Apellido
                        : "Paciente desconocido",
                    cita.Doctor != null
                        ? cita.Doctor.Nombre + " " + cita.Doctor.Apellido
                        : "Doctor no asignado",
                    cita.FechaInicio,
                    cita.Motivo,
                    cita.TipoConsulta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email de cancelación: {ex.Message}");
            }

            return Ok(new { message = "Cita cancelada correctamente." });
        }

        // =========================================================
        // HELPER — zona horaria El Salvador
        // ✅ Dentro de la clase, antes del último }
        private static TimeZoneInfo GetZonaElSalvador()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/El_Salvador");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            }
        }

    }  
}      

        



    

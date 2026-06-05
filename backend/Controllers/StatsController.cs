using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelMedAPI.Data;
using TelMedAPI.Helpers;

namespace TelMedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class StatsController : ControllerBase
    {
        private readonly TelMedAPIContext _context;

        public StatsController(TelMedAPIContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            // ============================
            // ÓRDENES LABORATORIO POR MES
            // ============================
            var ordenesRaw = await _context.Consultas
                .Where(c => c.TieneOrdenLaboratorio)
                .GroupBy(c => new
                {
                    Year = c.Fecha.Year,
                    Month = c.Fecha.Month
                })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    total = g.Count()
                })
                .OrderBy(x => x.year)
                .ThenBy(x => x.month)
                .ToListAsync();

            var ordenesPorMes = ordenesRaw.Select(x => new
            {
                mes = $"{x.month}/{x.year}",
                total = x.total
            });

            // ============================
            // INCAPACIDADES POR MES
            // ============================
            var incapacidadesRaw = await _context.Consultas
                .Where(c => c.TieneIncapacidad)
                .GroupBy(c => new
                {
                    Year = c.Fecha.Year,
                    Month = c.Fecha.Month
                })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    total = g.Count()
                })
                .OrderBy(x => x.year)
                .ThenBy(x => x.month)
                .ToListAsync();

            var incapacidadesPorMes = incapacidadesRaw.Select(x => new
            {
                mes = $"{x.month}/{x.year}",
                total = x.total
            });

            // ============================
            // DOCTORES MÁS SOLICITADOS
            // ============================
            var topDoctores = await _context.Citas
            .Include(c => c.Doctor)
            .Where(c => c.Doctor != null)
            .GroupBy(c => new
            {
                Year = c.FechaInicio.Year,
                Month = c.FechaInicio.Month,
                c.DoctorId,
                nombre = c.Doctor!.Nombre + " " + c.Doctor.Apellido
            })
            .Select(g => new
            {
                mes = $"{g.Key.Month}/{g.Key.Year}",
                doctor = g.Key.nombre,
                total = g.Count()
            })
            .OrderByDescending(x => x.total)
            .ToListAsync();

            // ============================
            // PACIENTES MÁS RECURRENTES
            // ============================
            var topPacientes = await _context.Citas
            .Include(c => c.Paciente)
            .GroupBy(c => new
            {
                Year = c.FechaInicio.Year,
                Month = c.FechaInicio.Month,
                c.PacienteId,
                nombre = c.Paciente.Nombre + " " + c.Paciente.Apellido
            })
            .Select(g => new
            {
                mes = $"{g.Key.Month}/{g.Key.Year}",
                paciente = g.Key.nombre,
                total = g.Count()
            })
            .OrderByDescending(x => x.total)
            .ToListAsync();

            // ============================
            // CITAS POR MES
            // ============================
            var citasRaw = await _context.Citas
                .GroupBy(c => new
                {
                    Year = c.FechaInicio.Year,
                    Month = c.FechaInicio.Month
                })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    total = g.Count()
                })
                .OrderBy(x => x.year)
                .ThenBy(x => x.month)
                .ToListAsync();

            var citasPorMes = citasRaw.Select(x => new
            {
                mes = $"{x.month}/{x.year}",
                total = x.total
            });

            return Ok(new
            {
                ordenesPorMes,
                incapacidadesPorMes,
                topDoctores,
                topPacientes,
                citasPorMes
            });
        }
    }
}
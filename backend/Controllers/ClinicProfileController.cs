using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelMedAPI.Data;
using TelMedAPI.DTOs;
using TelMedAPI.Models;

namespace TelMedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClinicProfileController : ControllerBase
    {
        private readonly TelMedAPIContext _context;

        public ClinicProfileController(TelMedAPIContext context)
        {
            _context = context;
        }

        // =========================================
        // GET: api/ClinicProfile
        [HttpGet]
        public async Task<ActionResult<ClinicProfileDTO>> GetClinicProfile()
        {
            var profile = await _context.ClinicProfiles.FirstOrDefaultAsync();

            // Si no existe, crear uno vacío automáticamente
            if (profile == null)
            {
                profile = new ClinicProfile
                {
                    ClinicName = "Mi Clínica",
                    Slogan = "Concectando con tu salud",
                    Horario = "Lunes a Viernes • 8:00 AM - 5:00 PM",
                    Telefono = "+503 0000-0000"
                };

                _context.ClinicProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var dto = new ClinicProfileDTO
            {
                ClinicName = profile.ClinicName,
                Slogan = profile.Slogan,
                Horario = profile.Horario,
                Telefono = profile.Telefono
            };

            return Ok(dto);
        }

        // =========================================
        // PUT: api/ClinicProfile
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateClinicProfile(UpdateClinicProfileDTO dto)
        {
            var profile = await _context.ClinicProfiles.FirstOrDefaultAsync();

            // Seguridad extra por si algo raro pasa
            if (profile == null)
            {
                profile = new ClinicProfile();
                _context.ClinicProfiles.Add(profile);
            }

            profile.ClinicName = dto.ClinicName;
            profile.Slogan = dto.Slogan;
            profile.Horario = dto.Horario;
            profile.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Perfil clínico actualizado correctamente"
            });
        }
    }
}
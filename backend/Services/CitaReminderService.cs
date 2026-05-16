using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelMedAPI.Data;
using TelMedAPI.Helpers;

namespace TelMedAPI.Services
{
    public class CitaReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CitaReminderService> _logger;

        public CitaReminderService(
            IServiceScopeFactory scopeFactory,
            ILogger<CitaReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EnviarRecordatoriosAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar recordatorios de citas.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // El servicio se detuvo.
                }
            }
        }

        private async Task EnviarRecordatoriosAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TelMedAPIContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            var ahoraUtc = DateTimeOffset.UtcNow;
            var ventanaInicio = ahoraUtc.AddHours(23);
            var ventanaFin = ahoraUtc.AddHours(25);

            var citas = await context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .Where(c => !c.NotificacionRecordatorioEnviada
                            && c.Estado != CitaEstados.Cancelada
                            && c.Estado != CitaEstados.Finalizada
                            && c.FechaInicio >= ventanaInicio
                            && c.FechaInicio <= ventanaFin)
                .ToListAsync(stoppingToken);

            foreach (var cita in citas)
            {
                if (cita.Paciente == null || string.IsNullOrWhiteSpace(cita.Paciente.Email))
                {
                    _logger.LogWarning("La cita {CitaId} no tiene paciente o correo válido.", cita.IdCita);
                    continue;
                }

                try
                {
                    await emailService.EnviarNotificacionRecordatorioCita(
                        cita.Paciente.Email,
                        cita.Paciente.Nombre + " " + cita.Paciente.Apellido,
                        cita.Doctor != null
                            ? cita.Doctor.Nombre + " " + cita.Doctor.Apellido
                            : "Doctor no asignado",
                        cita.FechaInicio,
                        cita.Motivo,
                        cita.TipoConsulta,
                        cita.LinkReunion);

                    cita.NotificacionRecordatorioEnviada = true;
                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo enviar el recordatorio de cita {CitaId}.", cita.IdCita);
                }
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TelMedAPI.Data;
using TelMedAPI.Helpers;

namespace TelMedAPI.Services
{
    public class CitaNoAsistidaService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CitaNoAsistidaService> _logger;

        public CitaNoAsistidaService(
            IServiceScopeFactory scopeFactory,
            ILogger<CitaNoAsistidaService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await MarcarNoAsistidas();
                // Revisar cada 5 minutos
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task MarcarNoAsistidas()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                              .GetRequiredService<TelMedAPIContext>();
            try
            {
                var limite = DateTime.UtcNow.AddMinutes(-60);

                // Citas que siguen Pendientes, o en consulta, pero ya pasaron hace más de 1 hora // se pasarán a no asistidas
                var citasNoAsistidas = await context.Citas
                    .Where(c =>
                        (c.Estado == CitaEstados.EnConsulta && c.FechaInicio <= limite)
                         || (c.Estado == CitaEstados.Pendiente && c.FechaInicio <= limite))
                    .ToListAsync();

                if (citasNoAsistidas.Any())
                {
                    foreach (var cita in citasNoAsistidas)
                    {
                        cita.Estado      = CitaEstados.NoAsistida;
                        cita.LinkReunion = null; // ← invalidar el link
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation(
                        "{Count} citas marcadas como NoAsistida.", 
                        citasNoAsistidas.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CitaNoAsistidaService.");
            }
        }
    }
}
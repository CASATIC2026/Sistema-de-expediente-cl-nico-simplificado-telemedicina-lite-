using Microsoft.EntityFrameworkCore;
using TelMedAPI.Models;

namespace TelMedAPI.Data
{
    public class TelMedAPIContext : DbContext
    {
        public TelMedAPIContext(DbContextOptions<TelMedAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<HorarioDoctor> HorariosDoctor { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(u => u.CitasComoPaciente)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Doctor)
                .WithMany(u => u.CitasComoDoctor)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de horarios disponibles
            modelBuilder.Entity<HorarioDoctor>().HasData(
                new HorarioDoctor
                {
                    IdHorario = 1,
                    DiaSemana = 1, // lunes
                    Activo = true,
                    HoraInicio = new TimeSpan(8, 0, 0),
                    HoraFin = new TimeSpan(16, 0, 0)
                },
                new HorarioDoctor
                {
                    IdHorario = 2,
                    DiaSemana = 2, // martes
                    Activo = true,
                    HoraInicio = new TimeSpan(8, 0, 0),
                    HoraFin = new TimeSpan(16, 0, 0)
                },
                new HorarioDoctor
                {
                    IdHorario = 3,
                    DiaSemana = 3, // miércoles
                    Activo = true,
                    HoraInicio = new TimeSpan(8, 0, 0),
                    HoraFin = new TimeSpan(16, 0, 0)
                },
                new HorarioDoctor
                {
                    IdHorario = 4,
                    DiaSemana = 4, // jueves
                    Activo = true,
                    HoraInicio = new TimeSpan(8, 0, 0),
                    HoraFin = new TimeSpan(16, 0, 0)
                },
                new HorarioDoctor
                {
                    IdHorario = 5,
                    DiaSemana = 5, // viernes
                    Activo = true,
                    HoraInicio = new TimeSpan(8, 0, 0),
                    HoraFin = new TimeSpan(16, 0, 0)
                },
                new HorarioDoctor
                {
                    IdHorario = 6,
                    DiaSemana = 6, // sábado
                    Activo = true,
                    HoraInicio = new TimeSpan(9, 0, 0),
                    HoraFin = new TimeSpan(12, 0, 0)
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);
        }
    }
}
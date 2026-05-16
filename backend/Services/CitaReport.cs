using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TelMedAPI.Models;
using System.Text.Json;
using System.IO;

namespace TelMedAPI.Services
{
    public class CitaReport : IDocument
    {
        public Cita CitaData { get; }
        public Consulta ConsultaData { get; }

        public CitaReport(Cita cita, Consulta consulta)
        {
            CitaData = cita;
            ConsultaData = consulta;
        }

        public void Compose(IDocumentContainer container)
        {
            var medicamentos = string.IsNullOrEmpty(ConsultaData.MedicamentosJson)
                ? new List<MedicamentoItem>()
                : JsonSerializer.Deserialize<List<MedicamentoItem>>(ConsultaData.MedicamentosJson);

            var zonaElSalvador = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(
                ConsultaData.Fecha.ToUniversalTime(),
                zonaElSalvador
            );

            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                "LogoTelmed.png"
            );

            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);

                // =========================================================
                // HEADER
                // =========================================================
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        // Logo + nombre sistema izquierda
                        row.RelativeItem().Column(left =>
                        {
                            if (File.Exists(logoPath))
                            {
                                left.Item()
                                    .Width(60)
                                    .Image(logoPath);
                            }

                            left.Item().Text("TelMed Lite™")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        // Título centrado
                        row.RelativeItem().AlignCenter().Text("RECETA MÉDICA")
                            .FontSize(22)
                            .Bold();

                        // Espacio visual derecha
                        row.RelativeItem();
                    });

                    header.Item().PaddingTop(20);
                    header.Item().LineHorizontal(1);
                });

                // =========================================================
                // CONTENT
                // =========================================================
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    // =====================================================
                    // DATOS PACIENTE + FECHA
                    // =====================================================
                    col.Item().Row(row =>
                    {
                        // IZQUIERDA
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(5);

                            left.Item().Text($"Paciente: {CitaData.Paciente?.Nombre} {CitaData.Paciente?.Apellido}");
                            left.Item().Text($"DUI: {CitaData.Paciente?.DUI}");
                            left.Item().Text($"Correo: {CitaData.Paciente?.Email}");
                            left.Item().Text($"Doctor: {CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}");
                        });

                        // DERECHA
                        row.RelativeItem().Column(right =>
                        {
                            right.Spacing(5);

                            right.Item().Text($"Fecha: {fechaLocal:dd/MM/yyyy}");

                            right.Item().Text(
                                $"Hora: {fechaLocal:h:mm tt}"
                                    .Replace("AM", "a.m")
                                    .Replace("PM", "p.m")
                            );
                        });
                    });

                    col.Item().PaddingTop(5);
                    col.Item().LineHorizontal(1);

                    // =====================================================
                    // DATOS MÉDICOS EN 2 COLUMNAS
                    // =====================================================
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // COLUMNA IZQUIERDA
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(10);

                            left.Item().Text("Síntomas:").Bold();
                            left.Item().Text(ConsultaData.Sintomas ?? "No especificado");

                            left.Item().Text("Diagnóstico:").Bold();
                            left.Item().Text(ConsultaData.Diagnostico ?? "No especificado");
                        });

                        row.ConstantItem(30);

                        // COLUMNA DERECHA
                        row.RelativeItem().Column(right =>
                        {
                            right.Spacing(10);

                            right.Item().Text("Tratamiento:").Bold();
                            right.Item().Text(ConsultaData.Tratamiento ?? "No especificado");

                            right.Item().Text("Observaciones:").Bold();
                            right.Item().Text(ConsultaData.Observaciones ?? "No especificado");
                        });
                    });

                    if (ConsultaData.TieneIncapacidad && ConsultaData.FechaInicioIncapacidad.HasValue && ConsultaData.FechaFinIncapacidad.HasValue)
                    {
                        var inicioIncapacidad = ConsultaData.FechaInicioIncapacidad.Value;
                        var finIncapacidad = ConsultaData.FechaFinIncapacidad.Value;
                        var diasIncapacidad = ConsultaData.DiasIncapacidad ?? (int)(finIncapacidad.Date - inicioIncapacidad.Date).TotalDays + 1;

                        col.Item().PaddingTop(15);
                        col.Item().Text("Incapacidad médica").Bold().FontSize(12);
                        col.Item().PaddingTop(5).Text($"Desde: {inicioIncapacidad:dd/MM/yyyy}");
                        col.Item().Text($"Hasta: {finIncapacidad:dd/MM/yyyy}");
                        col.Item().Text($"Días: {diasIncapacidad}");
                        col.Item().Text($"Motivo: {ConsultaData.MotivoIncapacidad}");

                        if (!string.IsNullOrWhiteSpace(ConsultaData.ObservacionesIncapacidad))
                        {
                            col.Item().Text($"Observaciones incapacidad: {ConsultaData.ObservacionesIncapacidad}");
                        }

                        col.Item().PaddingTop(10);
                        col.Item().LineHorizontal(1);
                    }

                    col.Item().PaddingTop(10);
                    col.Item().LineHorizontal(1);

                    // =====================================================
                    // MEDICAMENTOS
                    // =====================================================
                    col.Item().PaddingTop(10);
                    col.Item().Text("Medicamentos:").Bold();

                    if (medicamentos != null && medicamentos.Any())
                    {
                        foreach (var med in medicamentos)
                        {
                            col.Item().PaddingTop(5).Text(
                                $"• {med.nombre} - Dosis: {med.dosis} | Frecuencia: {med.frecuencia} | Duración: {med.duracion}"
                            );
                        }
                    }
                    else
                    {
                        col.Item().Text("No se especificaron medicamentos.");
                    }

                    col.Item().PaddingTop(10);
                    col.Item().LineHorizontal(1);

                    // =====================================================
                    // FIRMA
                    // =====================================================
                    col.Item().PaddingTop(60)
                        .AlignCenter()
                        .Column(signature =>
                        {
                            signature.Item().Text("____________________________")
                                .AlignCenter();

                            signature.Item().Text("Firma del médico")
                                .AlignCenter()
                                .FontSize(10);
                        });
                });

                // =========================================================
                // FOOTER
                // =========================================================
                page.Footer()
                    .AlignCenter()
                    .Text(txt =>
                    {
                        txt.Span("TelMed Lite™ System || Generado el ");
                        txt.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
            });
        }
    }

    public class MedicamentoItem
    {
        public string nombre { get; set; } = "";
        public string dosis { get; set; } = "";
        public string frecuencia { get; set; } = "";
        public string duracion { get; set; } = "";
    }
}
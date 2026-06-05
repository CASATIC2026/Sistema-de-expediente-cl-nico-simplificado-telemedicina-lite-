using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TelMedAPI.Models;

namespace TelMedAPI.Services
{
    public class IncapacidadReport : IDocument
    {
        public Cita CitaData { get; }
        public Consulta ConsultaData { get; }

        public IncapacidadReport(Cita cita, Consulta consulta)
        {
            CitaData = cita;
            ConsultaData = consulta;
        }

        public void Compose(IDocumentContainer container)
        {
            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                "LogoTelmed.png"
            );

            var fechaInicio = ConsultaData.FechaInicioIncapacidad!.Value;
            var fechaFin    = ConsultaData.FechaFinIncapacidad!.Value;
            var dias        = ConsultaData.DiasIncapacidad
                              ?? (int)(fechaFin.Date - fechaInicio.Date).TotalDays + 1;

            var zonaElSalvador = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var fechaEmision   = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaElSalvador);

            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);

                // ── HEADER ──────────────────────────────────────────────
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            if (File.Exists(logoPath))
                                left.Item().Width(60).Image(logoPath);

                            left.Item().Text("TelMed Lite™")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignCenter()
                            .Text("CERTIFICADO DE INCAPACIDAD MÉDICA")
                            .FontSize(18)
                            .Bold();

                        row.RelativeItem();
                    });

                    header.Item().PaddingTop(20);
                    header.Item().LineHorizontal(1);
                });

                // ── CONTENT ──────────────────────────────────────────────
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    // Datos paciente + doctor
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(5);
                            left.Item().Text($"Paciente: {CitaData.Paciente?.Nombre} {CitaData.Paciente?.Apellido}");
                            left.Item().Text($"DUI: {CitaData.Paciente?.DUI}");
                            left.Item().Text($"Correo: {CitaData.Paciente?.Email}");
                            left.Item().Text($"Doctor: {CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}");
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Spacing(5);
                            right.Item().Text($"Fecha de emisión: {fechaEmision:dd/MM/yyyy}");
                            right.Item().Text($"No. Consulta: {ConsultaData.IdConsulta}");
                        });
                    });

                    col.Item().PaddingTop(5);
                    col.Item().LineHorizontal(1);

                    // Cuerpo del certificado
                    col.Item().PaddingTop(15).Text(txt =>
                    {
                        txt.Span("El suscrito médico ");
                        txt.Span($"{CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}").Bold();
                        txt.Span(" hace constar que el/la paciente ");
                        txt.Span($"{CitaData.Paciente?.Nombre} {CitaData.Paciente?.Apellido}").Bold();
                        txt.Span(", portador(a) del DUI ");
                        txt.Span($"{CitaData.Paciente?.DUI}").Bold();
                        txt.Span(", requiere reposo médico por las siguientes razones:");
                    });

                    // Cuadro de datos de incapacidad
                    col.Item().PaddingTop(10)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten1)
                        .Padding(12)
                        .Column(box =>
                        {
                            box.Spacing(8);

                            box.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Motivo:").Bold();
                                row.RelativeItem(3).Text(ConsultaData.MotivoIncapacidad);
                            });

                            box.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Fecha inicio:").Bold();
                                row.RelativeItem(3).Text(fechaInicio.ToString("dd/MM/yyyy"));
                            });

                            box.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Fecha fin:").Bold();
                                row.RelativeItem(3).Text(fechaFin.ToString("dd/MM/yyyy"));
                            });

                            box.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Total de días:").Bold();
                                row.RelativeItem(3).Text($"{dias} día(s)");
                            });

                            if (!string.IsNullOrWhiteSpace(ConsultaData.ObservacionesIncapacidad))
                            {
                                box.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Observaciones:").Bold();
                                    row.RelativeItem(3).Text(ConsultaData.ObservacionesIncapacidad);
                                });
                            }
                        });

                    // Firma
                    col.Item().PaddingTop(80).AlignCenter().Column(sig =>
                    {
                        sig.Item().Text("____________________________").AlignCenter();
                        sig.Item().Text($"{CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}").AlignCenter().Bold();
                        sig.Item().Text("Médico Tratante").AlignCenter().FontSize(10);
                    });
                });

                // ── FOOTER ──────────────────────────────────────────────
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("TelMed Lite™ System || Generado el ");
                    txt.Span($"{fechaEmision:dd/MM/yyyy HH:mm}");
                });
            });
        }
    }
}
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TelMedAPI.Models;
using System.Text.Json;

namespace TelMedAPI.Services
{
    public class ExamenItem
    {
        public string nombre { get; set; } = "";
        public string indicaciones { get; set; } = "";
    }

    public class OrdenLaboratorioReport : IDocument
    {
        public Cita CitaData { get; }
        public Consulta ConsultaData { get; }

        public OrdenLaboratorioReport(Cita cita, Consulta consulta)
        {
            CitaData     = cita;
            ConsultaData = consulta;
        }

        public void Compose(IDocumentContainer container)
        {
            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                "LogoTelmed.png"
            );

            var examenes = string.IsNullOrEmpty(ConsultaData.ExamenesJson)
                ? new List<ExamenItem>()
                : JsonSerializer.Deserialize<List<ExamenItem>>(ConsultaData.ExamenesJson)
                  ?? new List<ExamenItem>();

            var zonaElSalvador = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var fechaEmision   = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaElSalvador);

            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);

                // ── HEADER ──────────────────────────────────────────
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
                            .Text("ORDEN DE LABORATORIO")
                            .FontSize(20)
                            .Bold();

                        row.RelativeItem();
                    });

                    header.Item().PaddingTop(20);
                    header.Item().LineHorizontal(1);
                });

                // ── CONTENT ─────────────────────────────────────────
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
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Spacing(5);
                            right.Item().Text($"Doctor: {CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}");
                            right.Item().Text($"Fecha de emisión: {fechaEmision:dd/MM/yyyy}");
                            right.Item().Text($"No. Consulta: {ConsultaData.IdConsulta}");
                        });
                    });

                    col.Item().PaddingTop(5);
                    col.Item().LineHorizontal(1);

                    // Indicación general
                    col.Item().PaddingTop(10).Text(txt =>
                    {
                        txt.Span("El/La Dr(a). ");
                        txt.Span($"{CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}").Bold();
                        txt.Span(" solicita la realización de los siguientes exámenes de laboratorio:");
                    });

                    // Tabla de exámenes
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);   // #
                            cols.RelativeColumn(3);    // Examen
                            cols.RelativeColumn(4);    // Indicaciones
                        });

                        // Encabezado
                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(6)
                                .Text("#").Bold().FontSize(10);
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(6)
                                .Text("Examen").Bold().FontSize(10);
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(6)
                                .Text("Indicaciones").Bold().FontSize(10);
                        });

                        // Filas
                        for (int i = 0; i < examenes.Count; i++)
                        {
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            var examen = examenes[i];

                            table.Cell().Background(bg).Padding(6)
                                .Text($"{i + 1}").FontSize(10);
                            table.Cell().Background(bg).Padding(6)
                                .Text(examen.nombre).FontSize(10);
                            table.Cell().Background(bg).Padding(6)
                                .Text(string.IsNullOrWhiteSpace(examen.indicaciones)
                                    ? "—"
                                    : examen.indicaciones)
                                .FontSize(10);
                        }
                    });

                    // Firma
                    col.Item().PaddingTop(80).AlignCenter().Column(sig =>
                    {
                        sig.Item().Text("____________________________").AlignCenter();
                        sig.Item().Text($"{CitaData.Doctor?.Nombre} {CitaData.Doctor?.Apellido}")
                            .AlignCenter().Bold();
                        sig.Item().Text("Médico Tratante").AlignCenter().FontSize(10);
                    });
                });

                // ── FOOTER ──────────────────────────────────────────
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("TelMed Lite™ System || Generado el ");
                    txt.Span($"{fechaEmision:dd/MM/yyyy HH:mm}");
                });
            });
        }
    }
}
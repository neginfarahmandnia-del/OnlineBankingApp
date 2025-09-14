using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfDocument = QuestPDF.Fluent.Document;
using OnlineBankingApp.Domain.Entities;

namespace OnlineBankingApp.Infrastructure.Services
{
    public class KontoauszugGenerator
    {
        public static byte[] Generate(IEnumerable<Transaction> transactions, string kontoName, DateTime von, DateTime bis)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return QuestPdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"Kontoauszug – {kontoName}").FontSize(20).Bold();
                        col.Item().Text($"Zeitraum: {von:dd.MM.yyyy} bis {bis:dd.MM.yyyy}").FontSize(12);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Datum
                            columns.RelativeColumn();    // Beschreibung
                            columns.ConstantColumn(80);  // Betrag
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Datum").Bold();
                            header.Cell().Text("Beschreibung").Bold();
                            header.Cell().Text("Betrag").Bold();
                        });

                        foreach (var tx in transactions)
                        {
                            table.Cell().Text(tx.Date.ToShortDateString());
                            table.Cell().Text(tx.Description ?? "-");
                            table.Cell().Text($"{tx.Amount:0.00} €").AlignRight();
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Seite ");
                        text.CurrentPageNumber();
                        text.Span(" von ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}

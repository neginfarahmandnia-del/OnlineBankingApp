using ClosedXML.Excel;
using OnlineBankingApp.Domain.Entities;
// ggf. using System.Linq;
using System.IO;
using System.Linq;
public class TransactionExportService
{
    public MemoryStream GenerateExcel(BankAccount bankAccount)
    {
        var stream = new MemoryStream();

        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("Transactions");

            // Daten als Tabelle schreiben (übersichtlicher als Zelle für Zelle)
            var rows = bankAccount.Transactions
                .OrderBy(t => t.Date)
                .Select(t => new
                {
                    t.Id,
                    Datum = t.Date,                 // wird gleich formatiert
                    Typ = t.Type.ToString(),
                    Betrag = t.Amount,
                    Kategorie = t.Category,
                    Beschreibung = t.Description
                });

            var table = ws.Cell(1, 1).InsertTable(rows, "Transactions", true);
            // Spaltenformatierung
            ws.Column(2).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Column(4).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();

            wb.SaveAs(stream);
        }

        stream.Position = 0;
        return stream;
    }
}

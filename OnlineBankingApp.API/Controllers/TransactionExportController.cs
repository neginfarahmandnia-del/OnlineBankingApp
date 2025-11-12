using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Infrastructure.Services;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace OnlineBankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionExportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TransactionExportService _exportService;

        public TransactionExportController(ApplicationDbContext context, TransactionExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

     
        [HttpGet("excel")]
        public IActionResult ExportExcel([FromQuery] int bankAccountId)
        {
            var bankAccount = _context.BankAccounts
                .Include(b => b.Transactions)
                .FirstOrDefault(b => b.Id == bankAccountId);

            if (bankAccount == null)
                return NotFound();

            var stream = _exportService.GenerateExcel(bankAccount);

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "export.xlsx"
            );
        }






        [HttpGet("pdf")]
        public IActionResult ExportPdf([FromQuery] int bankAccountId)
        {
            var konto = _context.BankAccounts
                .Include(k => k.Transactions)
                .FirstOrDefault(k => k.Id == bankAccountId);

            if (konto == null)
            {
                return NotFound($"Kein Konto mit Id {bankAccountId} gefunden.");
            }

            if (!konto.Transactions.Any())
            {
                return BadRequest("Dieses Konto hat noch keine Transaktionen.");
            }

            var pdfBytes = KontoauszugGenerator.Generate(
                konto.Transactions,
                konto.Name,
                DateTime.Now.AddMonths(-1),
                DateTime.Now
            );

            return File(pdfBytes, "application/pdf", "Kontoauszug.pdf");
        }



        // 3. Chart-Auswertung (LiveCharts, Chart.js oder Diagramm exportieren)
        // Beispiel: Monatsauswertung als JSON
        [HttpGet("monthly-chart-data")]
        public IActionResult GetChartData(int bankAccountId, int year, int month)
        {
            var data = _context.Transactions
                .Where(t => t.BankAccountId == bankAccountId && t.Date.Year == year && t.Date.Month == month)
                .GroupBy(t => t.Date.Day)
                .Select(g => new { Day = g.Key, Einnahmen = g.Where(x => x.Amount > 0).Sum(x => x.Amount), Ausgaben = g.Where(x => x.Amount < 0).Sum(x => x.Amount) })
                .OrderBy(x => x.Day)
                .ToList();

            return Ok(data);
        }

    }
}


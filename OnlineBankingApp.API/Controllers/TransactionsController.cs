using Microsoft.AspNetCore.Mvc;
using OnlineBankingApp.Application.Dtos.Transactions;
using OnlineBankingApp.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedDto = OnlineBankingApp.Shared.Models.TransactionDto;

namespace OnlineBankingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SharedDto>>> GetTransactions()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            var result = transactions.Select(t => new SharedDto
            {
                Id = t.Id,
                Description = t.Description ?? string.Empty,
                Amount = t.Amount,
                Date = t.Date,
                Category = t.Category ?? string.Empty
            });

            return Ok(result);
        }

        // GET: api/Transactions/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SharedDto>> GetTransaction(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            var dto = new SharedDto
            {
                Id = transaction.Id,
                Description = transaction.Description ?? string.Empty,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Category = transaction.Category ?? string.Empty
            };

            return Ok(dto);
        }

        // POST: api/Transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newTransactionId = await _transactionService.CreateTransactionAsync(request);

            return CreatedAtAction(nameof(GetTransaction), new { id = newTransactionId }, null);
        }

        // DELETE: api/Transactions/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var success = await _transactionService.DeleteTransactionAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Application.Dtos.Accounts;
using OnlineBankingApp.Application.Dtos;
namespace OnlineBankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransferController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
        {
            var success = await _transactionService.TransferAsync(dto);

            if (!success)
                return BadRequest("Überweisung fehlgeschlagen.");

            return Ok("Überweisung erfolgreich.");
        }
    }

}

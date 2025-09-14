using Microsoft.AspNetCore.Mvc;
using OnlineBankingApp.Application.Dtos.BankAccounts;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Domain.Entities;
using OnlineBankingApp.Infrastructure.Services;

namespace OnlineBankingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountsController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountsController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBankAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = new BankAccount
            {
                IBAN = dto.IBAN,
                Name = dto.Name,
                AccountHolder = dto.AccountHolder,
                WarnLimit = dto.WarnLimit,
                Kontotyp = dto.Kontotyp,     // jetzt verfügbar
                Abteilung = dto.Abteilung    // jetzt verfügbar
            };

            var created = await _bankAccountService.Create(account);

            var result = new BankAccountDto
            {
                Id = created.Id,
                IBAN = created.IBAN,
                Name = created.Name,
                AccountHolder = created.AccountHolder,
                Balance = created.Balance,
                WarnLimit = created.WarnLimit,
                 Kontotyp = created.Kontotyp,
                Abteilung = created.Abteilung
            };

            return Ok(result);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankAccountDto>>> GetAll()
        {
            var accounts = await _bankAccountService.GetAll();

            var result = accounts.Select(a => new BankAccountDto
            {
                Id = a.Id,
                IBAN = a.IBAN,
                Name = a.Name,
                AccountHolder = a.AccountHolder,
                Balance = a.Balance,
                WarnLimit = a.WarnLimit,
                Kontotyp = a.Kontotyp,
                Abteilung = a.Abteilung
            });

            return Ok(result);
        }

    }
}

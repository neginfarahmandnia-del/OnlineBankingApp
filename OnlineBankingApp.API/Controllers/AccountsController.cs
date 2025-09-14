using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Application.Dtos.BankAccounts;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Domain.Entities;
using OnlineBankingApp.Infrastructure.Identity;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Models.Dtos;
using System.Security.Claims;
using OnlineBankingApp.Application.Dtos.Accounts;
namespace OnlineBankingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ILogger<AccountsController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountsController(
        IBankAccountService bankAccountService,
        ILogger<AccountsController> logger,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _bankAccountService = bankAccountService;
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    private string GetUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    // 🔹 GET: api/Accounts/bankaccounts (alle Konten - Admin oder Übersicht)
    [HttpGet("bankaccounts")]
    public async Task<ActionResult<IEnumerable<BankAccountDto>>> GetBankAccounts()
    {
        var accounts = await _context.BankAccounts
            .Select(b => new BankAccountDto
            {
                Id = b.Id,
                IBAN = b.IBAN,
                Name = b.Name,
                AccountHolder = b.AccountHolder
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // 🔹 GET: api/Accounts/user-accounts (nur eigene Konten)
    [HttpGet("user-accounts")]
    public async Task<ActionResult<IEnumerable<BankAccountDto>>> GetUserBankAccounts()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var accounts = await _context.BankAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new BankAccountDto
            {
                Id = a.Id,
                IBAN = a.IBAN,
                Name = a.Name,
                AccountHolder = a.AccountHolder
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // 🔹 GET: api/Accounts/all (Direkt über Service)
    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BankAccount>>> GetAll()
    {
        var accounts = await _bankAccountService.GetAll();
        return Ok(accounts);
    }
    // OnlineBankingApp.API/Controllers/AccountsController.cs

    [HttpPost] // POST api/Accounts
    public async Task<IActionResult> CreateAccount([FromBody] CreateBankAccountDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // 1 Konto pro Benutzer – prüfen
        var already = await _context.BankAccounts.AnyAsync(a => a.UserId == userId);
        if (already) return Conflict("Dieser Benutzer hat bereits ein Konto.");

        var account = new BankAccount
        {
            UserId = userId,
            IBAN = dto.IBAN,
            Name = dto.Name,
            AccountHolder = dto.AccountHolder,
            Kontotyp = dto.Kontotyp,
            Abteilung = dto.Abteilung,
            WarnLimit = dto.WarnLimit,

            Balance = 0m,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _bankAccountService.Create(account);
        // optional: created zurückgeben (oder nur 201 ohne Body)
        return CreatedAtAction(nameof(GetUserBankAccounts), new { id = created.Id }, created);
    }

    // 🔹 GET: api/Accounts/balance
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var balance = await _bankAccountService.GetBalanceAsync(userId);
        return Ok(new { balance });
    }

    // 🔹 POST: api/Accounts/deposit
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Der Betrag muss positiv sein.");

        var userId = GetUserId();
        var result = await _bankAccountService.DepositAsync(userId, dto.Amount, dto.Description);

        return result ? Ok("Einzahlung erfolgreich") : BadRequest("Einzahlung fehlgeschlagen");
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Der Betrag muss positiv sein.");

        var userId = GetUserId();
        var result = await _bankAccountService.WithdrawAsync(userId, dto.Amount, dto.Description);

        return result ? Ok("Auszahlung erfolgreich") : BadRequest("Auszahlung fehlgeschlagen");
    }


    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Der Betrag muss positiv sein.");

        var userId = GetUserId();
        var result = await _bankAccountService.TransferAsync(
            userId,
            dto.FromAccountId,
            dto.ToAccountId,
            dto.Amount,
            dto.Description);

        return result ? Ok("Transfer erfolgreich") : BadRequest("Transfer fehlgeschlagen");
    }


    // 🔹 GET: api/Accounts/transactions
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var transactions = await _bankAccountService.GetTransactionHistoryAsync(userId);
        return Ok(transactions);
    }
}

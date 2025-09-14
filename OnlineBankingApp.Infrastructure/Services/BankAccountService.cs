using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Domain.Entities;
using OnlineBankingApp.Infrastructure.Persistence;

namespace OnlineBankingApp.Infrastructure.Services;

public class BankAccountService : IBankAccountService
{
    private readonly ApplicationDbContext _context;

    public BankAccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BankAccount>> GetAll()
    {
        return await _context.BankAccounts
            .Include(b => b.Transactions)
            .ToListAsync();
    }

    public async Task<BankAccount> Create(BankAccount account)
    {
     
        account.CreatedAt = DateTime.UtcNow;
        account.Balance = 0;

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    public async Task<BankAccount> CreateAccountAsync(string userId)
    {
        var account = new BankAccount
        {
          
            UserId = userId,
            Balance = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<decimal> GetBalanceAsync(string userId)
    {
        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);
        return account?.Balance ?? 0m;
    }

    public async Task<bool> DepositAsync(string userId, decimal amount, string? description = null)
    {
        if (amount <= 0) return false;

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);
        if (account == null) return false;

        account.Balance += amount;

        _context.Transactions.Add(new Transaction
        {
           
            BankAccountId = account.Id,
            Amount = amount,
            Type = TransactionType.Deposit,
            Description = description,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WithdrawAsync(string userId, decimal amount, string? description = null)
    {
        if (amount <= 0) return false;

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);
        if (account == null || account.Balance < amount) return false;

        account.Balance -= amount;

        _context.Transactions.Add(new Transaction
        {
           
            BankAccountId = account.Id,
            Amount = amount,
            Type = TransactionType.Withdrawal,
            Description = description,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TransferAsync(string userId, int fromAccountId, int toAccountId, decimal amount, string? description)
    {
        if (amount <= 0) return false;

        var fromAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == fromAccountId && a.UserId == userId);
        var toAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == toAccountId);

        if (fromAccount == null || toAccount == null)
            return false;

        if (fromAccount.Balance < amount)
            return false;

        fromAccount.Balance -= amount;
        toAccount.Balance += amount;

        var timestamp = DateTime.UtcNow;

        _context.Transactions.Add(new Transaction
        {
            BankAccountId = fromAccount.Id,
            Amount = -amount,
            Type = TransactionType.Transfer,
            Timestamp = timestamp,
            Description = $"Transfer an Konto {toAccount.Id}: {description}"
        });

        _context.Transactions.Add(new Transaction
        {
            BankAccountId = toAccount.Id,
            Amount = amount,
            Type = TransactionType.Transfer,
            Timestamp = timestamp,
            Description = $"Empfangen von Konto {fromAccount.Id}: {description}"
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Transaction>> GetTransactionHistoryAsync(string userId)
    {
        var account = await _context.BankAccounts
            .Include(b => b.Transactions)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        return account?.Transactions
                   .OrderByDescending(t => t.Timestamp)
                   .ToList()
               ?? new List<Transaction>();
    }
}

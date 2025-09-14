using OnlineBankingApp.Domain.Entities;

namespace OnlineBankingApp.Application.Interfaces;

public interface IBankAccountService
{
    Task<List<BankAccount>> GetAll();
    Task<BankAccount> Create(BankAccount account);
    Task<BankAccount> CreateAccountAsync(string userId);
    Task<decimal> GetBalanceAsync(string userId);
    Task<bool> DepositAsync(string userId, decimal amount, string? description = null);
    Task<bool> WithdrawAsync(string userId, decimal amount, string? description = null);
    Task<List<Transaction>> GetTransactionHistoryAsync(string userId);
    Task<bool> TransferAsync(string userId, int fromAccountId, int toAccountId, decimal amount, string? description);
}

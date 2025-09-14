using OnlineBankingApp.Application.Dtos.Transactions;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Domain.Entities;
using OnlineBankingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineBankingApp.Application.Dtos;
using OnlineBankingApp.Application.Dtos.Accounts;
namespace OnlineBankingApp.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _db;

        public TransactionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TransactionDto>> GetAllTransactionsAsync()
        {
            var transactions = await _db.Transactions
                .Include(t => t.BankAccount)
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                BankAccountId = t.BankAccountId,
                Amount = t.Amount,
                Date = t.Date,
                Description = t.Description ?? string.Empty,
                Type = t.Type.ToString(),
                AccountHolder = t.BankAccount != null ? t.BankAccount.AccountHolder : "Unbekannt"
            }).ToList();
        }


        public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
        {
            var t = await _db.Transactions
                .Include(t => t.BankAccount)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (t == null || t.BankAccount == null)
                return null;

            return new TransactionDto
            {
                Id = t.Id,
                BankAccountId = t.BankAccountId,
                Amount = t.Amount,
                Date = t.Date,
                Description = t.Description ?? string.Empty,
                Type = t.Type.ToString(),
                AccountHolder = t.BankAccount.AccountHolder
            };
        }

        public async Task<int> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var transaction = new Transaction
            {
                BankAccountId = request.BankAccountId,
                Amount = request.Amount,
                Date = request.Date,
                Description = request.Description ?? string.Empty,
                Type = request.Type, // hier direkt übernehmen
                CreatedAt = DateTime.UtcNow
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return transaction.Id;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _db.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();

            return true;
        }


        public async Task<bool> TransferAsync(TransferRequestDto dto)
        {
            var from = await _db.BankAccounts.FindAsync(dto.FromAccountId);
            var to = await _db.BankAccounts.FindAsync(dto.ToAccountId);

            if (from == null || to == null || from.Balance < dto.Amount)
                return false;

            var now = DateTime.UtcNow;

            from.Balance -= dto.Amount;
            to.Balance += dto.Amount;

            var withdrawalTransaction = new Transaction
            {
                BankAccountId = from.Id,
                Amount = -dto.Amount,
                Date = now,
                Description = dto.Description ?? "Überweisung",
                Type = TransactionType.Withdrawal,
                CreatedAt = now
            };

            var depositTransaction = new Transaction
            {
                BankAccountId = to.Id,
                Amount = dto.Amount,
                Date = now,
                Description = dto.Description ?? "Überweisung (Eingang)",
                Type = TransactionType.Deposit,
                CreatedAt = now
            };

            _db.Transactions.AddRange(withdrawalTransaction, depositTransaction);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}

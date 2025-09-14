using OnlineBankingApp.Application.Dtos;
using OnlineBankingApp.Application.Dtos.Transactions;
using OnlineBankingApp.Application.Dtos.Accounts;
namespace OnlineBankingApp.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<bool> TransferAsync(TransferRequestDto dto);
        Task<List<TransactionDto>> GetAllTransactionsAsync();
        Task<TransactionDto?> GetTransactionByIdAsync(int id);
        Task<int> CreateTransactionAsync(CreateTransactionRequest request);
        Task<bool> DeleteTransactionAsync(int id); // ✅ hinzugefügt
    }
}

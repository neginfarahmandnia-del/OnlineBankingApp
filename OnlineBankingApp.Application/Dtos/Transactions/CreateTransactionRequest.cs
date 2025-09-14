using OnlineBankingApp.Domain.Entities;

namespace OnlineBankingApp.Application.Dtos.Transactions
{
    public class CreateTransactionRequest
    {
        public int BankAccountId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }   
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; } // ⬅️ Das muss vorhanden sein!
    }
}

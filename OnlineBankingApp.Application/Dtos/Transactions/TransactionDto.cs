using OnlineBankingApp.Domain.Entities;

public class TransactionDto
{
    public int Id { get; set; }
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
}

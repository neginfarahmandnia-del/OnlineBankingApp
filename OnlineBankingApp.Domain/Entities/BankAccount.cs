using System.ComponentModel.DataAnnotations;


namespace OnlineBankingApp.Domain.Entities;

public class BankAccount
{
    [Key]
    public int  Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public decimal WarnLimit { get; set; } = 50;
    public string AccountHolder { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty; // ❗️Diese Zeile muss existieren
                                                     // NEU
    public string Kontotyp { get; set; } = string.Empty;
    public string Abteilung { get; set; } = string.Empty;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Beispiel-Namespace
using OnlineBankingApp.Domain.Models; // Oder dein tatsächlicher Pfad
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankingApp.Domain.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int  BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;

    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime Date { get; set; }
}

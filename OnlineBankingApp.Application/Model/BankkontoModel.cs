using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace OnlineBankingApp.Application.Models;

public class BankkontoModel
{
    [Required(ErrorMessage = "Der Kontoinhaber ist erforderlich.")]
    public string? InhaberName { get; set; }

    [Required(ErrorMessage = "Ein Kontotyp muss gewählt werden.")]
    public string? Kontotyp { get; set; }

    [Required(ErrorMessage = "Eine Abteilung muss gewählt werden.")]
    public string? Abteilung { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Das Startguthaben muss positiv sein.")]
    public decimal Startguthaben { get; set; }
}

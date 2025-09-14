using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OnlineBankingApp.Domain.Entities;

public enum TransactionType
{
    [Display(Name = "Einzahlung")]
    Deposit = 0,

    [Display(Name = "Auszahlung")]
    Withdrawal = 1,

    [Display(Name = "Überweisung")]
    Transfer = 2
}
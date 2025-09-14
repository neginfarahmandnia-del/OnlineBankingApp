using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Application.Dtos.BankAccounts
{
    public class DepositRequestDto
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; } // optional

    }
}

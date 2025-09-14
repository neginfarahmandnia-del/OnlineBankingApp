using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Application.Dtos.Accounts
{
    public class DepositWithdrawRequest
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }

    }
}

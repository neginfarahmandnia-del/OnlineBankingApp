using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Application.Dtos.TransactionHistory
{
    public class FilterTransactionHistoryRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Limit { get; set; }
    }
}

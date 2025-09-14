using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Application.Dtos.TransactionHistory
{
    public class TransactionHistoryDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

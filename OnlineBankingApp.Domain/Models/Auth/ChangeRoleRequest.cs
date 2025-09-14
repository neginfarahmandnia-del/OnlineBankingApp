using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Domain.Models.Auth
{
    public class ChangeRoleRequest
    {
        public string NewRole { get; set; } = string.Empty;
    }
}

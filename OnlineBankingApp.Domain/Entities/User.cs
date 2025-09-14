using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Domain.Entities
{
    // Domain/Entities/User.cs  (nur relevante Teile)
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        // ...
        public int? RoleId { get; set; }
        public Role? Role { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Domain.Entities
{
    // Domain/Entities/Role.cs
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "Admin", "Manager", ...
        public List<User> Users { get; set; } = new();
    }

    

}

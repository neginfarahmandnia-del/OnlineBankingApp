using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBankingApp.Domain.Entities
{
    public class Abteilung
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

    }

}

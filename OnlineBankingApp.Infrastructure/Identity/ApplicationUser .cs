using Microsoft.AspNetCore.Identity;
using OnlineBankingApp.Domain.Entities;

namespace OnlineBankingApp.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public int? AbteilungId { get; set; }
        public Abteilung? Abteilung { get; set; }
    }
}

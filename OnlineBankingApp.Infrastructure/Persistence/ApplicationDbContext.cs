using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Infrastructure.Identity;
using OnlineBankingApp.Domain.Entities;
using OnlineBankingApp.Application.Common.Interfaces;

namespace OnlineBankingApp.Infrastructure.Persistence
{
    // KEINE eigene Role-Entity hier – IdentityRole wird verwendet
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<Abteilung> Abteilungen { get; set; } = null!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);

        // NUR EINE OnModelCreating!
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Abteilungen Seed
            modelBuilder.Entity<Abteilung>().HasData(
                new Abteilung { Id = 1, Name = "Kundenservice" },
                new Abteilung { Id = 2, Name = "Kreditabteilung" },
                new Abteilung { Id = 3, Name = "Compliance" },
                new Abteilung { Id = 4, Name = "IT" },
                new Abteilung { Id = 5, Name = "Marketing" }
            );

            // BankAccount
            modelBuilder.Entity<BankAccount>(e =>
            {
                e.HasIndex(x => x.IBAN)
     
     .HasFilter("[UserId] IS NOT NULL AND [UserId] <> ''");

                e.Property(x => x.IBAN).HasMaxLength(34).IsRequired();
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.AccountHolder).HasMaxLength(100).IsRequired();
                e.Property(x => x.Kontotyp).HasMaxLength(50).IsRequired();
                e.Property(x => x.Abteilung).HasMaxLength(100).IsRequired();
                e.Property(x => x.Balance).HasPrecision(18, 4);
                e.Property(x => x.WarnLimit).HasPrecision(18, 2);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Transaction
            modelBuilder.Entity<Transaction>(e =>
            {
                e.Property(t => t.Amount).HasPrecision(18, 4);
                e.Property(t => t.Type).HasConversion<string>();
                e.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}

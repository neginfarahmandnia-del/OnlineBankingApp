using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OnlineBankingApp.Infrastructure.Persistence;

namespace OnlineBankingApp.Infrastructure;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // ⚠️ Verbindung zu deiner Datenbank hier anpassen!
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=OnlineBankingDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

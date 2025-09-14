using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


using OnlineBankingApp.Domain.Entities;

namespace OnlineBankingApp.Application.Common.Interfaces { 


    public interface IApplicationDbContext
    {
        DbSet<BankAccount> BankAccounts { get; }
        DbSet<Transaction> Transactions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Infrastructure.Services;
using OnlineBankingApp.Domain.Entities; // Für BankAccount
using OnlineBankingApp.Infrastructure.Identity; // Für ApplicationUser

namespace OnlineBankingApp.Infrastructure.Services
{
    public class KontostandPruefer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KontostandPruefer> _logger;

        public KontostandPruefer(IServiceScopeFactory scopeFactory, ILogger<KontostandPruefer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var kritischeKonten = context.BankAccounts
                    .Where(k => k.Balance < k.WarnLimit)
                    .ToList();

                foreach (var konto in kritischeKonten)
                {
                    var user = await userManager.FindByIdAsync(konto.UserId);

                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await emailService.SendEmailAsync(
                            user.Email,
                            "Kontowarnung",
                            $"Ihr Kontostand ({konto.Name}) ist unter {konto.WarnLimit:0.00} € gefallen.");
                    }
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}

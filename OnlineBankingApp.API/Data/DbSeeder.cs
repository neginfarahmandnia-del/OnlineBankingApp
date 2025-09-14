using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineBankingApp.Infrastructure.Identity;


namespace OnlineBankingApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedTestUser(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("test@example.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "test@example.com",
                    Email = "test@example.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, "Test123!");
            }
        }
    }
}

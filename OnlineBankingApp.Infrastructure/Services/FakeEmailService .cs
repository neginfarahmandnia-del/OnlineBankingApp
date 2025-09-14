namespace OnlineBankingApp.Infrastructure.Services
{
    public class FakeEmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            Console.WriteLine($"Fake email to: {to}, subject: {subject}, body: {body}");
            return Task.CompletedTask;
        }
    }
}

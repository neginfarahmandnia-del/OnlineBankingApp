namespace OnlineBankingAdminPanel.Services
{
    public interface IProtectedApiService
    {
        Task<string?> GetCurrentUserEmailAsync();
    }

}

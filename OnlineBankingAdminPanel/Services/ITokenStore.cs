// Services/TokenStore.cs
namespace OnlineBankingAdminPanel.Services
{
    public interface ITokenStore
    {
        string? Token { get; set; }
    }

    public sealed class TokenStore : ITokenStore
    {
        public string? Token { get; set; }
    }
}

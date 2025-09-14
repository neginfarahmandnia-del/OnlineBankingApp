namespace OnlineBankingAdminPanel.Models.Auth
{
    public class LoginResult
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static LoginResult SuccessResult() => new() { Success = true };
        public static LoginResult Failed(string error) => new() { Success = false, ErrorMessage = error };
    }

}

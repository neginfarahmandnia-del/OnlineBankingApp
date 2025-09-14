namespace OnlineBankingApp.Shared.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AbteilungName { get; set; }
        public int? AbteilungId { get; set; }
    }

    public class ChangeUserAbteilungRequest
    {
        public int? AbteilungId { get; set; }
    }
}

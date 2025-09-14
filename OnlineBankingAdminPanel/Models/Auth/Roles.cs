// OnlineBankingAdminPanel/Auth/Roles.cs
namespace OnlineBankingAdminPanel.Auth
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Mitarbeiter = "Mitarbeiter";

        // Für Policies/Sichtbarkeit
        public static readonly string[] CanManageUsers = { Admin, Manager };
    }
}

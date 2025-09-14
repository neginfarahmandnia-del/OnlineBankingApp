// OnlineBankingAdminPanel/Auth/ClaimsPrincipalExtensions.cs
using System.Linq;
using System.Security.Claims;

namespace OnlineBankingAdminPanel.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
            => roles.Any(user.IsInRole);
    }
}

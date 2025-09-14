using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace OnlineBankingAdminPanel.Services
{
    /// <summary>
    /// Liest das JWT aus der Session ("authToken"), parst Claims (inkl. Rollen),
    /// und liefert einen AuthenticationState für Blazor.
    /// </summary>
    public sealed class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private const string TokenKey = "authToken";
        private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
        private readonly ProtectedSessionStorage _session;

        public CustomAuthStateProvider(ProtectedSessionStorage session)
        {
            _session = session;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var result = await _session.GetAsync<string>(TokenKey);
                var token = result.Success ? result.Value : null;

                if (string.IsNullOrWhiteSpace(token))
                    return new AuthenticationState(Anonymous);

                var claims = ParseClaimsFromJwt(token);
                // Optional: Token-Expiry prüfen (exp)
                if (IsExpired(claims))
                {
                    await _session.DeleteAsync(TokenKey);
                    return new AuthenticationState(Anonymous);
                }

                var identity = new ClaimsIdentity(claims, authenticationType: "JwtAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(Anonymous);
            }
        }

        /// <summary>
        /// Beim Login aufrufen: Token speichern und State aktualisieren.
        /// </summary>
        public async Task SetTokenAsync(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
            {
                await LogoutAsync();
                return;
            }

            await _session.SetAsync(TokenKey, jwt);

            var claims = ParseClaimsFromJwt(jwt);
            var identity = new ClaimsIdentity(claims, "JwtAuth");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task LogoutAsync()
        {
            await _session.DeleteAsync(TokenKey);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
        }

        // ----------------- Helpers -----------------

        private static bool IsExpired(IEnumerable<Claim> claims)
        {
            var exp = claims.FirstOrDefault(c => c.Type is "exp");
            if (exp == null) return false;
            if (!long.TryParse(exp.Value, out var seconds)) return false;

            // exp ist UnixTimeSeconds (UTC)
            var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return expiryUtc <= DateTimeOffset.UtcNow;
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            // JWT: header.payload.signature (Base64Url)
            var parts = jwt.Split('.');
            if (parts.Length < 2) return Array.Empty<Claim>();

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            var claims = new List<Claim>();

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name)
                {
                    // Rollen können als "role": "Admin" oder "roles": ["Admin","Manager"] kommen
                    case "role":
                    case "roles":
                        AddRoleClaims(prop.Value, claims);
                        break;

                    // Standard-Claims
                    case "unique_name":
                    case "name":
                    case "sub":
                    case "email":
                        claims.Add(new Claim(ClaimTypes.Name, prop.Value.GetString() ?? ""));
                        break;

                    default:
                        // Roh-Claim mitschleppen (auch "exp" etc.)
                        claims.Add(new Claim(prop.Name, prop.Value.ToString()));
                        break;
                }
            }

            // Falls kein Name gesetzt wurde, wenigstens was Sinnvolles setzen
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                var email = claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "user";
                claims.Add(new Claim(ClaimTypes.Name, email));
            }

            return claims;
        }

        private static void AddRoleClaims(JsonElement el, List<Claim> claims)
        {
            if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    var role = item.GetString();
                    if (!string.IsNullOrWhiteSpace(role))
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else
            {
                var role = el.GetString();
                if (!string.IsNullOrWhiteSpace(role))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        private static byte[] Base64UrlDecode(string input)
        {
            // Base64Url -> Base64
            string output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4)
            {
                case 2: output += "=="; break;
                case 3: output += "="; break;
            }
            return Convert.FromBase64String(output);
        }
    }
}

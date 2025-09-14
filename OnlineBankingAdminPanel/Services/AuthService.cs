using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OnlineBankingAdminPanel.Models.Auth;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnlineBankingAdminPanel.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _session;
        private readonly ITokenStore _store;
        private const string TokenKey = "authToken";

        public AuthService(HttpClient http, ProtectedSessionStorage session, ITokenStore store)
        {
            _http = http;
            _session = session;
            _store = store;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var r = await _http.PostAsJsonAsync("api/auth/register", request);

                if (r.IsSuccessStatusCode)
                    return new AuthResult { Success = true };

                var body = await r.Content.ReadAsStringAsync();
                var reason = ExtractApiError(body) ?? r.ReasonPhrase ?? "Unbekannter Fehler";
                return new AuthResult { Success = false, ErrorMessage = $"Registrierung fehlgeschlagen: {reason}" };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, ErrorMessage = $"Fehler bei Registrierung: {ex.Message}" };
            }
        }

        /// <summary>
        /// Versucht, Identity-/ModelState-Fehler aus dem Response-Body zu extrahieren.
        /// Unterstützt Arrays von IdentityErrors, ModelState-Dictionaries und einfache message/title Felder.
        /// </summary>
        private static string? ExtractApiError(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // 1) Array von { code, description }
                if (root.ValueKind == JsonValueKind.Array)
                {
                    var msgs = new List<string>();
                    foreach (var el in root.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.Object)
                        {
                            if (el.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String)
                                msgs.Add(d.GetString()!);
                            else if (el.TryGetProperty("Description", out var d2) && d2.ValueKind == JsonValueKind.String)
                                msgs.Add(d2.GetString()!);
                        }
                        else if (el.ValueKind == JsonValueKind.String)
                        {
                            msgs.Add(el.GetString()!);
                        }
                    }
                    if (msgs.Count > 0) return string.Join("; ", msgs);
                }

                // 2) Objekt mit "errors" (ModelState) oder "message"/"title"
                if (root.ValueKind == JsonValueKind.Object)
                {
                    // ModelState-Fehler: { errors: { "Password": ["...","..."], "Email": ["..."] } }
                    if (root.TryGetProperty("errors", out var errors))
                    {
                        var msgs = new List<string>();
                        if (errors.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in errors.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var item in prop.Value.EnumerateArray())
                                        if (item.ValueKind == JsonValueKind.String)
                                            msgs.Add(item.GetString()!);
                                }
                                else if (prop.Value.ValueKind == JsonValueKind.String)
                                {
                                    msgs.Add(prop.Value.GetString()!);
                                }
                            }
                        }
                        else if (errors.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in errors.EnumerateArray())
                                if (item.ValueKind == JsonValueKind.String)
                                    msgs.Add(item.GetString()!);
                        }
                        if (msgs.Count > 0) return string.Join("; ", msgs);
                    }

                    if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                        return msg.GetString();

                    if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        return title.GetString();
                }
            }
            catch
            {
                // Body war kein JSON, einfach Rohtext zurückgeben
            }

            // Fallback: Rohtext zeigen (kann bei einfachen APIs schon helfen)
            return body;
        }


        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            try
            {
                var r = await _http.PostAsJsonAsync("api/auth/login", request);
                if (!r.IsSuccessStatusCode)
                    return new AuthResult { Success = false, ErrorMessage = $"Login fehlgeschlagen: {r.StatusCode}" };

                var body = await r.Content.ReadAsStringAsync();
                string? token = TryExtractToken(body);

                if (string.IsNullOrWhiteSpace(token))
                    return new AuthResult { Success = false, ErrorMessage = $"Kein Token erhalten. Antwort: {body}" };

                await SetTokenAsync(token);   // -> Store + Session
                return new AuthResult { Success = true };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, ErrorMessage = $"Fehler beim Login: {ex.Message}" };
            }
        }

        public async Task LogoutAsync() => await RemoveTokenAsync();

        public async Task<string?> GetTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_store.Token))
                return _store.Token;

            try
            {
                var res = await _session.GetAsync<string>(TokenKey);
                return res.Success ? res.Value : null;
            }
            catch { return null; }
        }

        public async Task SetTokenAsync(string token)
        {
            _store.Token = token;                  // interop-frei
            await _session.SetAsync(TokenKey, token);
        }

        public async Task RemoveTokenAsync()
        {
            _store.Token = null;
            await _session.DeleteAsync(TokenKey);
        }

        // ---- helpers ----
        private static string? TryExtractToken(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 1) bekannte Feldnamen
                var keys = new[] { "token", "Token", "accessToken", "access_token", "jwt", "jwtToken", "id_token" };
                foreach (var k in keys)
                    if (root.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.String)
                        return el.GetString();

                // 2) plain string?
                if (root.ValueKind == JsonValueKind.String)
                    return root.GetString();

                // 3) irgendein string-Wert, der wie ein JWT aussieht
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(s) && LooksLikeJwt(s))
                            return s;
                    }
                }
            }
            catch { /* ignore */ }
            return null;
        }

        private static bool LooksLikeJwt(string? s)
            => !string.IsNullOrWhiteSpace(s) && s.Count(c => c == '.') == 2;
    }
}

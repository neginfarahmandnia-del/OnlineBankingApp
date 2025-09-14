using OnlineBankingAdminPanel.Services;
using System.Net.Http.Json;
using System.Text.Json;

public class ProtectedApiService : IProtectedApiService
{
    private readonly HttpClient _http;

    public ProtectedApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> GetCurrentUserEmailAsync()
    {
        try
        {
            // Debug-Ausgabe: rohe JSON-Antwort anzeigen
            var rawJson = await _http.GetStringAsync("api/auth/me");
            Console.WriteLine($"🧪 API gibt zurück: {rawJson}");

            // Dann den Inhalt als UserInfo deserialisieren
            var userInfo = JsonSerializer.Deserialize<UserInfo>(rawJson);

            return userInfo?.Email;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler beim Abrufen des Benutzers: {ex.Message}");
            return null;
        }
    }


}

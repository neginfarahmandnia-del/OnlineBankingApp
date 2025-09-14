using OnlineBankingAdminPanel.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<HttpResponseMessage> CreateBankkontoAsync(BankkontoDto dto)
    {
        return await _http.PostAsJsonAsync("api/bankkonten", dto);
    }

    public async Task<string[]> GetKontoTypenAsync()
    {
        return await _http.GetFromJsonAsync<string[]>("api/bankkonten/kontotypen") ?? [];
    }

    public async Task<string[]> GetAbteilungenAsync()
    {
        return await _http.GetFromJsonAsync<string[]>("api/bankkonten/abteilungen") ?? [];
    }
}

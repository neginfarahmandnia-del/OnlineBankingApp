using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace OnlineBankingApp.Tests
{
    public class TransferTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TransferTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Transfer_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            var token = await GetTestJwtTokenAsync(); // Diese Hilfsmethode musst du noch implementieren
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = "/api/Accounts/transfer?toUserEmail=nonexistent@example.com&amount=50";

            // Act
            var response = await _client.PostAsync(url, null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<string> GetTestJwtTokenAsync()
        {
            // TODO: Diese Methode muss angepasst werden, um einen gültigen JWT vom Login zu bekommen
            // Beispielhaft:
            var loginData = new
            {
                email = "testuser@example.com",
                password = "Test123!"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginData);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result?.Token ?? throw new Exception("Token not found");
        }

        private class LoginResponse
        {
            public string Token { get; set; } = default!;
        }
    }
}

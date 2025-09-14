using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OnlineBankingAdminPanel.Services
{
    /// <summary>
    /// Hängt das JWT (falls vorhanden) an jeden Request des "AuthenticatedClient".
    /// </summary>
    public sealed class AuthMessageHandler : DelegatingHandler
    {
        private readonly ITokenStore _store;
        private readonly ProtectedSessionStorage _session;
        private readonly ILogger<AuthMessageHandler> _logger;

        public AuthMessageHandler(
            ITokenStore store,
            ProtectedSessionStorage session,
            ILogger<AuthMessageHandler> logger)
        {
            _store = store;
            _session = session;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1) Schnell & sicher: In-Memory-Store
            string? token = _store.Token;

            // 2) Fallback: SessionStorage (kann bei Prerender/fehlendem Circuit fehlschlagen)
            if (string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var res = await _session.GetAsync<string>("authToken");
                    token = res.Success ? res.Value : null;
                }
                catch
                {
                    token = null;
                }
            }

            bool present = !string.IsNullOrWhiteSpace(token);
            _logger.LogInformation("AuthHandler -> {Method} {Url} | Token present: {Present}",
                request.Method, request.RequestUri, present);

            if (present && request.Headers.Authorization is null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

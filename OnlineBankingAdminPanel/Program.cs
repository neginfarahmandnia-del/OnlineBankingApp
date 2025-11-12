using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.HttpOverrides;
using OnlineBankingAdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// === API-Basisadresse lesen (Reihenfolge: appsettings → Env → Fallback lokal) ===
var apiBase =
    builder.Configuration["ApiBaseUrl"] ??
    Environment.GetEnvironmentVariable("ApiBaseUrl") ??
    "https://localhost:7202/"; // lokal als Fallback (Achtung: Slash am Ende!)

if (!apiBase.EndsWith("/")) apiBase += "/";

// ---------- Blazor & Auth ----------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin", "Manager"));
});

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// ---------- HTTP-Clients ----------
// Unauthentifizierter HttpClient (falls benötigt)
builder.Services.AddHttpClient("ApiClient", c =>
{
    c.BaseAddress = new Uri(apiBase);
});

// Authentifizierter Client mit JWT-Handler
builder.Services.AddScoped<AuthMessageHandler>();
builder.Services.AddHttpClient("AuthenticatedClient", c =>
{
    c.BaseAddress = new Uri(apiBase);
}).AddHttpMessageHandler<AuthMessageHandler>();

// Falls irgendwo ein "HttpClient" direkt injiziert wird, zeige auf die API-Base:
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });

// ---------- App-Services ----------
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProtectedApiService, ProtectedApiService>(); // optional

// ---------- Logging ----------
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ---------- Pipeline ----------
var app = builder.Build();

// Forwarded headers (wichtig hinter Render/Proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// In Produktion Fehlerseite, lokal Dev-Exception-Page
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// HTTPS-Redirect kann hinter Render stören. Wenn nötig, auskommentieren.
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

// Health-Endpoint für Render
app.MapGet("/healthz", () => Results.Ok("OK"));

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

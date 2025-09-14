using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OnlineBankingAdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Blazor & Auth ----------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore(options =>
{
    // Admin ODER Manager darf z. B. Benutzer verwalten
    options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin", "Manager"));
});

// Auth-State + Session-Storage
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// ---------- HTTP-Clients ----------
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7202/") // API-Basisadresse
});

builder.Services.AddScoped<AuthMessageHandler>();
builder.Services.AddHttpClient("AuthenticatedClient", c =>
{
    c.BaseAddress = new Uri("https://localhost:7202/");
}).AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddHttpClient("ApiClient", c =>
{
    c.BaseAddress = new Uri("https://localhost:7202/");
});

// ---------- App-Services ----------
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProtectedApiService, ProtectedApiService>(); // optional

// ---------- Logging ----------
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ---------- Pipeline ----------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

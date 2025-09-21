OnlineBankingApp

Eine kleine, aber vollständige Online-Banking Demo mit ASP.NET Core 8, EF Core (SQL Server), ASP.NET Identity + JWT und einem Blazor Server Admin Panel.
Ziele: saubere Architektur, realistische Features (Konten, Transaktionen, Rollen/Abteilungen), Export (Excel/PDF) und einfache lokale Inbetriebnahme.

Inhaltsverzeichnis

Features

Technologien

Architektur & Projektstruktur

Schnellstart

Standard-Logins & Seeding

Wichtige Endpunkte

Rollen & Berechtigungen

Berichte & Exporte

Konfiguration

Troubleshooting

Screenshots

Lizenz

Features

Registrierung & Login (JWT)

Rollen/Policies: Admin, Manager, Mitarbeiter + Policy CanManageUsers

Benutzerverwaltung im Admin Panel:

Benutzer anzeigen/löschen

Abteilung zuweisen

Rollenverwaltung (Liste, anlegen, löschen)

Bankkonten: anlegen, anzeigen (pro Benutzer), optional: 1 Konto pro Benutzer

Transaktionen: Einzahlung, Auszahlung, Transfer (mit Buchungen)

Berichte: Monatsdiagramm via GET /api/TransactionExport/monthly-chart-data

Export:

Excel (/api/TransactionExport/excel)

PDF-Kontoauszug (/api/TransactionExport/pdf)

Swagger/OpenAPI für die API

Technologien

Backend (API): ASP.NET Core 8 (Web API), ASP.NET Identity, JWT (Bearer)

Datenbank: SQL Server, EF Core

Admin UI: Blazor Server

Export: ClosedXML (Excel), eigener PDF-Generator

Auth: JWT mit Claims (NameIdentifier, Name, Role)

Logging: Microsoft.Extensions.Logging

Architektur & Projektstruktur
OnlineBankingApp.sln
README.md
OnlineBankingApp.API/            # Web API (Auth, Users, BankAccounts, Exporte, Reports)
OnlineBankingAdminPanel/         # Blazor Server Admin-Frontend (Users, Roles, Reports, Konto anlegen)
OnlineBankingApp.Infrastructure/ # EF Core DbContext, Services (BankAccountService, Export, Seeder)
OnlineBankingApp.Application/    # Interfaces, DTOs, Policies
OnlineBankingApp.Domain/         # Entities (BankAccount, Transaction, ApplicationUser, ...)
OnlineBankingApp.Shared/         # Geteilte DTOs für UI/API


Wichtige Controller (API):

AuthController: /api/auth/login, /api/auth/register, /api/auth/me

UsersController: /api/users, /api/users/{id}, /api/users/roles, PUT .../abteilung

BankAccountsController: /api/bankaccounts (CRUD/Read), benutzergebunden

TransactionExportController: Exporte + Monatsdaten

Schnellstart
Voraussetzungen

.NET 8 SDK

SQL Server (lokal/Express/Docker)

(Optional) EF Core Tools: dotnet tool install --global dotnet-ef

1) Repository klonen
git clone <dein-repo-url>
cd OnlineBankingApp

2) Verbindung & JWT konfigurieren

In OnlineBankingApp.API/appsettings.json:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=OnlineBankingDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "ersetze-das-durch-einen-langen-sicheren-key"
  }
}

3) Datenbank erstellen/migrieren
# im Projektordner der API
dotnet ef database update

4) Projekte starten

API starten ? läuft lokal z. B. auf https://localhost:7202

AdminPanel starten ? läuft lokal z. B. auf https://localhost:7203

Die API zeigt Swagger unter https://localhost:7202/swagger.

Standard-Logins & Seeding

Beim Start der API werden Rollen (Admin, Manager, Mitarbeiter) sowie ein Admin-User angelegt:

Admin: 1admin@example.com

Passwort: Admin23!

Du kannst dich im AdminPanel einloggen und sofort die Benutzer-/Rollenverwaltung testen.

Wichtige Endpunkte

Auth

POST /api/auth/register – Registrierung

POST /api/auth/login – Login ? { token }

GET /api/auth/me – Claims prüfen (auth nötig)

Benutzer & Rollen (Admin/Manager Policy)

GET /api/users

PUT /api/users/{id}/abteilung ? { AbteilungId | Abteilung }

GET /api/users/roles – Rollenliste

(Optional) PUT /api/auth/change-role/{userId} ? { newRole }

Konten

POST /api/bankaccounts – Konto anlegen (benutzergebunden)

GET /api/bankaccounts – eigene Konten abrufen

GET /api/bankaccounts/{id}

Exporte & Berichte

GET /api/TransactionExport/excel?bankAccountId={id} ? Excel

GET /api/TransactionExport/pdf?bankAccountId={id} ? PDF

GET /api/TransactionExport/monthly-chart-data?bankAccountId={id}&year=2025&month=9 ? Diagrammdaten

Rollen & Berechtigungen

Rollen: Admin, Manager, Mitarbeiter

Policy: CanManageUsers ? RequireRole("Admin","Manager")

AdminPanel Pages:

/users (Benutzerverwaltung) ? [Authorize(Roles="Admin,Manager")]

/roles (Rollenverwaltung) ? [Authorize(Roles="Admin")]

/bankkonto (Konto anlegen) ? [Authorize]

Abteilung kann im AdminPanel unter Benutzerverwaltung pro Benutzer zugewiesen/geändert werden.

Berichte & Exporte

Reports (AdminPanel/Reports.razor) nutzt
GET /api/TransactionExport/monthly-chart-data
um Einnahmen/Ausgaben pro Tag eines Monats anzuzeigen.

Excel-Export via ClosedXML

PDF-Kontoauszug via eigenem Generator (KontoauszugGenerator)

Konfiguration
Key	Ort	Beschreibung
ConnectionStrings:DefaultConnection	API appsettings.json	SQL Server Verbindung
Jwt:Key	API appsettings.json	Geheimer Key für JWT Signatur
API BaseAddress	Admin Program.cs/HttpClient	z. B. https://localhost:7202/

Ports: per launchSettings.json der jeweiligen Projekte.

Troubleshooting
401/403 im AdminPanel (z. B. /users)

Stelle sicher, dass der Login im AdminPanel erfolgreich war und das JWT im ProtectedSessionStorage liegt.

In der API: AddAuthentication().AddJwtBearer(...) ist korrekt konfiguriert, inkl.

options.TokenValidationParameters = new TokenValidationParameters {
    // ...
    NameClaimType = ClaimTypes.Name,
    RoleClaimType = ClaimTypes.Role
};


Der aufgerufene Endpunkt verlangt ggf. Rolle/Policy (z. B. Admin/Manager).

Konto anlegen: „duplicate key… IX_BankAccounts_UserId“

Du hast (optional) 1 Konto pro Benutzer erzwungen (Unique-Index).

Entweder gewünschtes Verhalten beibehalten,

oder Migration anpassen und den Index nicht uniek machen.

Rollen löschen: 404

Prüfe, ob dein Rollen-Endpoint und der Frontend-Pfad exakt übereinstimmen
(z. B. DELETE /api/roles/{roleName} vs. DELETE /api/users/roles/{id}).

Swagger erreichbar?

https://localhost:7202/swagger ? Wenn nicht sichtbar: app.UseSwagger(); app.UseSwaggerUI(); im Development aktiv?

Screenshots

## Screenshots

![Login](docs/images/login.png)
![Benutzerverwaltung](docs/images/users.png)
![Dashboard](docs/images/Dashboard.png)
![Bankkonto eröffnen](docs/images/Bankkonto eröffnen.png)
![Abteilungen](docs/images/Abteilungen.png)
![Export & Charts](docs/images/Export & Charts.png)
![Home Page der Admin Panel](docs/images/Home Page der Admin Panel.png)
![Home Page der Swagger UI](docs/images/Home Page der Swagger UI.png)
![Rollen](docs/images/Rollen.png)
![Transaktionen](docs/images/Transaktionen.png)

Lizenz

---

# `docker-compose.yml` (Projektwurzel)

```yaml
version: '3.9'
services:
  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_strong_password123
    ports:
      - "1433:1433"
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "Your_strong_password123", "-C", "-Q", "SELECT 1"]
      interval: 5s
      timeout: 3s
      retries: 30

  api:
    build: ./OnlineBankingApp.API
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=sql;Database=OnlineBanking;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True
      - Jwt__Key=please_change_me_32_chars_min
    ports:
      - "7202:8080"
    depends_on:
      sql:
        condition: service_healthy

  admin:
    build: ./OnlineBankingAdminPanel
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ApiBaseUrl=https://host.docker.internal:7202/
    ports:
      - "7203:8080"
    depends_on:
      - api
Tests
cd tests/OnlineBankingApp.Tests
dotnet test
Serilog-Konfig
OnlineBankingApp.API/appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OnlineBanking;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "please_change_me_32_chars_min"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" }
    ],
    "Enrich": [ "FromLogContext" ]
  },
  "AllowedHosts": "*"
}
OnlineBankingApp.API/Program.cs – Serilog & FluentValidation (ergänzen)
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
// ...

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OnlineBankingApp.Application.Dtos.BankAccounts.CreateBankAccountDtoValidator>();

// ... (deine restliche Registrierung für DbContext, Identity, JWT, Swagger)

var app = builder.Build();

app.UseSerilogRequestLogging(); // Request-Log

// ... (Middleware-Pipeline)
app.Run();
NuGet: Serilog.AspNetCore, Serilog.Sinks.Console, FluentValidation, FluentValidation.AspNetCore.

FluentValidation
OnlineBankingApp.Application/Dtos/BankAccounts/CreateBankAccountDtoValidator.cs
using FluentValidation;

namespace OnlineBankingApp.Application.Dtos.BankAccounts
{
    public class CreateBankAccountDtoValidator : AbstractValidator<CreateBankAccountDto>
    {
        public CreateBankAccountDtoValidator()
        {
            RuleFor(x => x.IBAN)
                .NotEmpty().WithMessage("IBAN ist erforderlich.")
                .Length(15, 34).WithMessage("IBAN-Länge muss zwischen 15 und 34 liegen.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kontoname ist erforderlich.")
                .MaximumLength(100);

            RuleFor(x => x.AccountHolder)
                .NotEmpty().WithMessage("Kontoinhaber ist erforderlich.")
                .MaximumLength(100);

            RuleFor(x => x.WarnLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Warnlimit darf nicht negativ sein.");

            // Optional: wenn du nur bestimmte Strings erlauben willst
            RuleFor(x => x.Kontotyp)
                .MaximumLength(50);

            RuleFor(x => x.Abteilung)
                .MaximumLength(50);
        }
    }
}

Unit-Tests
tests/OnlineBankingApp.Tests/OnlineBankingApp.Tests.csproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.7" />
    <PackageReference Include="FluentValidation" Version="11.9.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OnlineBankingApp.Infrastructure\OnlineBankingApp.Infrastructure.csproj" />
    <ProjectReference Include="..\..\OnlineBankingApp.Application\OnlineBankingApp.Application.csproj" />
    <ProjectReference Include="..\..\OnlineBankingApp.Domain\OnlineBankingApp.Domain.csproj" />
  </ItemGroup>
</Project>
tests/OnlineBankingApp.Tests/BankAccountServiceTests.cs
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Infrastructure.Services;
using OnlineBankingApp.Domain.Entities;
using FluentAssertions;
using Xunit;

public class BankAccountServiceTests
{
    private static ApplicationDbContext NewCtx()
    {
        var opt = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"db_{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(opt);
    }

    [Fact]
    public async Task Deposit_Increases_Balance_And_Writes_Transaction()
    {
        await using var ctx = NewCtx();
        var svc = new BankAccountService(ctx);

        var acc = await svc.Create(new BankAccount { UserId = "U1", IBAN="DE1234567890", Name="Giro", AccountHolder="Max" });

        var ok = await svc.DepositAsync("U1", 100m, "Start");
        ok.Should().BeTrue();

        var reloaded = await ctx.BankAccounts.Include(b=>b.Transactions).FirstAsync();
        reloaded.Balance.Should().Be(100m);
        reloaded.Transactions.Should().ContainSingle(t => t.Amount == 100m);
    }

    [Fact]
    public async Task Withdraw_Fails_When_Insufficient_Balance()
    {
        await using var ctx = NewCtx();
        var svc = new BankAccountService(ctx);

        await svc.Create(new BankAccount { UserId = "U1", IBAN="DE123", Name="Giro", AccountHolder="Max" });
        var ok = await svc.WithdrawAsync("U1", 50m, "Test");
        ok.Should().BeFalse();
    }
}
tests/OnlineBankingApp.Tests/CreateBankAccountDtoValidatorTests.cs
using FluentValidation.TestHelper;
using OnlineBankingApp.Application.Dtos.BankAccounts;
using Xunit;

public class CreateBankAccountDtoValidatorTests
{
    [Fact]
    public void Should_Have_Error_When_IBAN_Empty()
    {
        var v = new CreateBankAccountDtoValidator();
        var m = new CreateBankAccountDto { IBAN = "", Name="N", AccountHolder="A", WarnLimit=0 };
        var res = v.TestValidate(m);
        res.ShouldHaveValidationErrorFor(x => x.IBAN);
    }

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        var v = new CreateBankAccountDtoValidator();
        var m = new CreateBankAccountDto { IBAN = "DE123456789012345", Name="Giro", AccountHolder="Max", WarnLimit=0 };
        v.TestValidate(m).ShouldNotHaveAnyValidationErrors();
    }
}

GitHub Actions CI
.github/workflows/ci.yml
name: ci
on:
  push:
  pull_request:

jobs:
  build-test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore -c Release
      - name: Test
        run: dotnet test --no-build -c Release --verbosity normal
Paginierung für Users (API)
[HttpGet]
public async Task<IActionResult> GetUsers([FromQuery]int page=1, [FromQuery]int pageSize=20, [FromQuery]string? q=null)
{
    var query = _userManager.Users.AsQueryable();
    if (!string.IsNullOrWhiteSpace(q))
        query = query.Where(u => u.Email!.Contains(q));

    var total = await query.CountAsync();
    var items = await query
        .OrderBy(u => u.Email)
        .Skip((page-1)*pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Ok(new { total, page, pageSize, items = items.Select(u => new { u.Id, u.Email }) });
}
AsNoTracking bei reinen Reads
var accounts = await _context.BankAccounts
    .AsNoTracking()
    .Where(a => a.UserId == userId)
    .ToListAsync();




Kontakt
Negin Farahmandnia-Email:neginfarahmandnia@gmail.com
Hinweise für Reviewer

Clean Architecture light: Domain/Infrastructure/Application getrennt.

Security: JWT, Rollen/Policies, Controller-Attribute.

DX: Swagger, Seed-Daten, klare Fehlermeldungen, einfache lokale Inbetriebnahme.

UI: Blazor Server, responsiv, klare Admin-Flows (Users, Roles, Reports, Konto).

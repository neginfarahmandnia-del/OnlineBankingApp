using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineBankingApp.Application.Common.Interfaces;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Infrastructure.Identity;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Infrastructure.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
// optionaler Seeder:
using OnlineBankingApp.Domain.Entities;

namespace OnlineBankingApp.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;
            var env = builder.Environment;

            // -------------------------------------------------
            // DB
            // -------------------------------------------------
            var useInMemory = string.Equals(
                Environment.GetEnvironmentVariable("USE_INMEMORY_DB"),
                "true", StringComparison.OrdinalIgnoreCase);

            if (useInMemory)
            {
                builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseInMemoryDatabase("DemoDb"));
            }
            else
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection fehlt.");

                builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseSqlServer(connStr));
            }

            // -------------------------------------------------
            // Identity / Auth
            // -------------------------------------------------
            builder.Services
                .AddIdentityCore<ApplicationUser>(opts =>
                {
                    opts.Password.RequiredLength = 8;
                    opts.Password.RequireDigit = true;
                    opts.Password.RequireUppercase = true;
                    opts.Password.RequireLowercase = true;
                    opts.Password.RequireNonAlphanumeric = true;
                    opts.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin", "Manager"));
            });

            var jwtKey = config["Jwt:Key"] ?? throw new Exception("Jwt:Key nicht konfiguriert.");
            builder.Services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            // -------------------------------------------------
            // CORS
            // -------------------------------------------------
            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("AllowWeb", p => p
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:5174",
                        "http://127.0.0.1:5173",
                        "https://onlinebankingapp-2.onrender.com"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            // -------------------------------------------------
            // MVC / JSON
            // -------------------------------------------------
            builder.Services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // -------------------------------------------------
            // App-Services
            // -------------------------------------------------
            builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            builder.Services.AddScoped<IBankAccountService, BankAccountService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IEmailService, FakeEmailService>();
            builder.Services.AddScoped<TransactionExportService>();
            builder.Services.AddHostedService<KontostandPruefer>();

            // Secrets lokal zulassen
            builder.Configuration.AddUserSecrets<Program>();

            // -------------------------------------------------
            // Swagger
            // -------------------------------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineBanking API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // -------------------------------------------------
            // DB Migration + Seeding
            // -------------------------------------------------
            using (var scope = app.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var logger = sp.GetRequiredService<ILogger<Program>>();

                if (!useInMemory)
                {
                    var db = sp.GetRequiredService<ApplicationDbContext>();
                    await db.Database.MigrateAsync();
                }

                await SeedRolesAndAdminUser(sp, logger);

                // Optional: Demo-Konto + Transaktion für den Admin anlegen
                var seedAdminAccount = string.Equals(
                    Environment.GetEnvironmentVariable("SEED_ADMIN_ACCOUNT"),
                    "true", StringComparison.OrdinalIgnoreCase);

                if (seedAdminAccount)
                    await EnsureAdminHasAccountWithSampleData(sp, logger);
            }

            // -------------------------------------------------
            // Pipeline
            // -------------------------------------------------
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineBanking API v1");
                c.RoutePrefix = "swagger";
            });

            // app.UseHttpsRedirection(); // hinter Proxy optional
            app.UseRouting();
            app.UseCors("AllowWeb");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/", () => Results.Ok("OK"));
            app.MapGet("/healthz", () => Results.Ok("OK"));

            await app.RunAsync();
        }

        private static async Task SeedRolesAndAdminUser(IServiceProvider services, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Manager", "Mitarbeiter" })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            const string adminEmail = "1admin@example.com";
            const string adminPassword = "Admin23!";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                var res = await userManager.CreateAsync(admin, adminPassword);
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation("Admin-Benutzer erstellt.");
                }
                else
                {
                    logger.LogError("Fehler beim Erstellen Admin: {Errors}",
                        string.Join(", ", res.Errors.Select(e => e.Description)));
                }
            }
        }

        // -------- Optionaler Seeder: Konto + Testdaten für den Admin --------
        private static async Task EnsureAdminHasAccountWithSampleData(IServiceProvider services, ILogger logger)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            var admin = await userManager.FindByEmailAsync("1admin@example.com");
            if (admin is null) return;

            var account = await db.BankAccounts.FirstOrDefaultAsync(a => a.UserId == admin.Id);
            if (account is null)
            {
                account = new BankAccount
                {
                    UserId = admin.Id,
                    IBAN = "DE44500105175407323491",
                    Name = "Girokonto Privat",
                    AccountHolder = "Admin",
                    Kontotyp = "Giro",
                    Abteilung = "IT",
                    WarnLimit = 0,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow
                };
                db.BankAccounts.Add(account);
                await db.SaveChangesAsync();
                logger.LogInformation("Admin-Konto angelegt (Id {Id}).", account.Id);

                db.Transactions.Add(new Transaction
                {
                    BankAccountId = account.Id,
                    Amount = 150m,
                    Type = TransactionType.Deposit,
                    Description = "Startguthaben",
                    Category = "Sonstiges",
                    Date = DateTime.UtcNow.Date,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                logger.LogInformation("Beispiel-Transaktion angelegt.");
            }
        }
    }
}

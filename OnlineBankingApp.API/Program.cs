using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineBankingApp.Infrastructure.Identity;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Infrastructure.Services;
using OnlineBankingApp.Application.Common.Interfaces;
using OnlineBankingApp.Application.Interfaces;
using System.Text;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;

namespace OnlineBankingApp.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            // Umschalter: InMemory-DB
            var useInMemory = string.Equals(
                Environment.GetEnvironmentVariable("USE_INMEMORY_DB"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            // ---------------- DB ----------------
            if (useInMemory)
            {
                builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseInMemoryDatabase("DemoDb"));
            }
            else
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new InvalidOperationException("No ConnectionStrings:DefaultConnection configured and USE_INMEMORY_DB != true.");

                builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseSqlServer(connStr));
            }

            // -------------- Identity ------------
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

            // -------------- AuthZ ----------------
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin", "Manager"));
            });

            // -------------- JWT ------------------
            var jwtKey = config["Jwt:Key"] ?? throw new Exception("JWT key not set (Jwt:Key).");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            // ---------- Controller + JSON --------
            builder.Services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // --------- App-Services --------------
            builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            builder.Services.AddScoped<IBankAccountService, BankAccountService>();
            builder.Services.AddScoped<IEmailService, FakeEmailService>();
            builder.Services.AddScoped<TransactionExportService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IGreetingService, GreetingService>();
            builder.Services.AddHostedService<KontostandPruefer>();

            // --- Swagger (einmalig registrieren) ---
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineBanking API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Bearer: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Achtung: Dieser HttpClient zeigt auf localhost – in der Cloud meist nutzlos
            builder.Services.AddHttpClient("BackendAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:5001/api/");
            });

            var app = builder.Build();

            // Proxy-Header (Render)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // ---------- Seeding ----------
            using (var scope = app.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var logger = sp.GetRequiredService<ILogger<Program>>();

                if (useInMemory)
                {
                    await Data.DbSeeder.SeedTestUser(sp);
                    await SeedRolesAndAdminUser(sp, logger);
                }
                else
                {
                    var db = sp.GetRequiredService<ApplicationDbContext>();
                    await db.Database.MigrateAsync();
                    await SeedRolesAndAdminUser(sp, logger);
                }
            }

            // ---------- Pipeline ----------
            // 👉 Swagger IMMER aktivieren, nicht nur Development
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineBanking API v1");
                c.RoutePrefix = "swagger"; // /swagger & /swagger/index.html
            });

            // HTTPS-Redirect kann hinter Render stören – optional deaktivieren:
            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Health & Root
            app.MapGet("/", () => Results.Ok("OK"));
            app.MapGet("/healthz", () => Results.Ok("OK"));

            await app.RunAsync();
        }

        private static async Task SeedRolesAndAdminUser(IServiceProvider services, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Manager", "Mitarbeiter" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = "1admin@example.com";
            var adminPassword = "Admin23!";

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
    }
}

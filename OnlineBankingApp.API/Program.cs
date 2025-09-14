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

namespace OnlineBankingApp.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            // ---------------- DB ----------------
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // -------------- Identity ------------
            // -------------- Identity ------------
            // -------------- IdentityCore (ohne Cookies) ------------
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
                .AddSignInManager()           // für _signInManager.CheckPasswordSignInAsync(...)
                .AddDefaultTokenProviders();


            // -------------- AuthZ (Policies) ----
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CanManageUsers", p => p.RequireRole("Admin", "Manager"));
            });

            // -------------- JWT AuthN -----------
            var jwtKey = config["Jwt:Key"] ?? throw new Exception("JWT key not set in config");

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

                    // wichtig für [Authorize(Roles="...")]
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
            builder.Services.AddHttpClient("BackendAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:5001/api/");
            });

            // -------------- Swagger --------------
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

            var app = builder.Build();

            // ---------- Seeding ----------
            using (var scope = app.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;
                await Data.DbSeeder.SeedTestUser(sp);
            }
            using (var scope = app.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var logger = sp.GetRequiredService<ILogger<Program>>();
                await SeedRolesAndAdminUser(sp, logger);
            }

            // ---------- Pipeline ----------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();   // vor Authorization!
            app.UseAuthorization();
            app.MapControllers();

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

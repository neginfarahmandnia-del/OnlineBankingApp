// OnlineBankingApp.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineBankingApp.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineBankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        // 🔒 Aktueller User inkl. Rollen
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Name);

            IList<string> roles = Array.Empty<string>();
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    roles = await _userManager.GetRolesAsync(user);
            }

            return Ok(new { userId, email, roles });
        }

        // 🔒 Nur für Admins (Beispiel)
        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            return Ok("Admin sieht alle Benutzer.");
        }

        // 🔓 Rollenliste (hier nur Beispielwerte)
        [HttpGet("roles")]
        [Authorize(Roles = "Admin")] // ggf. entfernen, wenn öffentlich
        public IActionResult GetAllRoles()
        {
            var roles = new List<string> { "Admin", "Manager", "Customer" };
            return Ok(roles);
        }

        // 📥 Registrierung
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Customer");
            return Ok("Registrierung erfolgreich.");
        }

        // 🔐 Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login fehlgeschlagen – ModelState invalid: {@ModelState}", ModelState);
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Benutzer nicht gefunden: {Email}", model.Email);
                return Unauthorized(new { message = "Benutzer nicht gefunden." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Falsches Passwort für Benutzer {Email}", model.Email);
                return Unauthorized(new { message = "Passwort inkorrekt." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            _logger.LogInformation("Benutzer {Email} erfolgreich eingeloggt", model.Email);
            return Ok(new { token });
        }

        // 🔄 Rollen ändern – nur Admin
        [Authorize(Roles = "Admin")]
        [HttpPut("change-role/{userId}")]
        public async Task<IActionResult> ChangeUserRole(string userId, [FromBody] ChangeRoleRequest model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Benutzer nicht gefunden.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return BadRequest("Fehler beim Entfernen der alten Rolle.");

            var addResult = await _userManager.AddToRoleAsync(user, model.NewRole);
            if (!addResult.Succeeded) return BadRequest("Fehler beim Zuweisen der neuen Rolle.");

            return Ok($"Rolle für Benutzer {user.Email} geändert zu {model.NewRole}.");
        }

        // 📦 JWT
        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not configured.");
            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
                throw new Exception("Jwt:Key must be at least 32 bytes.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email ?? user.UserName ?? string.Empty)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddHours(8),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // 🔧 DTOs
    public class RegisterRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class ChangeRoleRequest
    {
        public string NewRole { get; set; } = string.Empty;
    }
}

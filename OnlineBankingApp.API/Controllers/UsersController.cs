using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Infrastructure.Identity;
using OnlineBankingApp.Infrastructure.Persistence;

namespace OnlineBankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CanManageUsers")] // optional
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public UsersController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public record UserVm(string Id, string? Email, int? AbteilungId, List<string> Roles);
        public class SetRoleRequest { public string? RoleName { get; set; } }
        public class SetAbteilungRequest { public int? AbteilungId { get; set; } }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserVm>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                // Falls du AbteilungId am User speicherst, hier setzen. Sonst null.
                list.Add(new UserVm(u.Id, u.Email, null, roles.ToList()));
            }
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();
            var roles = await _userManager.GetRolesAsync(u);
            return Ok(new UserVm(u.Id, u.Email, null, roles.ToList()));
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> SetRole(string id, [FromBody] SetRoleRequest req)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();

            var current = await _userManager.GetRolesAsync(u);
            if (current.Count > 0)
            {
                var rm = await _userManager.RemoveFromRolesAsync(u, current);
                if (!rm.Succeeded) return BadRequest(rm.Errors);
            }

            if (!string.IsNullOrWhiteSpace(req.RoleName))
            {
                if (!await _roleManager.RoleExistsAsync(req.RoleName))
                    return BadRequest($"Role '{req.RoleName}' does not exist.");
                var add = await _userManager.AddToRoleAsync(u, req.RoleName);
                if (!add.Succeeded) return BadRequest(add.Errors);
            }

            return NoContent();
        }

        [HttpPut("{id}/abteilung")]
        public async Task<IActionResult> SetAbteilung(string id, [FromBody] SetAbteilungRequest req)
        {
            // Beispiel-Implementierung:
            // Wenn du Abteilung am User speicherst, hier setzen und speichern.
            // Wenn Abteilung über BankAccount geht, musst du _db.BankAccounts filtern.
            // Vorläufiger NoContent, damit dein UI funktioniert:
            await Task.CompletedTask;
            return NoContent();
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();
            return Ok(roles);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u is null) return NotFound();
            var res = await _userManager.DeleteAsync(u);
            if (!res.Succeeded) return BadRequest(res.Errors);
            return NoContent();
        }
    }
}

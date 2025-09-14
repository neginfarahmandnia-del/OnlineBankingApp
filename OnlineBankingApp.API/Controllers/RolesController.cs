using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.Infrastructure.Identity;

namespace OnlineBankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager,
                               UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET /api/roles  -> liefert { id, name }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            return Ok(list);
        }

        public record CreateRoleRequest(string Name);

        // POST /api/roles  -> { name: "Admin" }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Name))
                return BadRequest("Role name is required.");

            var name = req.Name.Trim();
            if (await _roleManager.RoleExistsAsync(name))
                return Conflict($"Role '{name}' already exists.");

            var res = await _roleManager.CreateAsync(new IdentityRole(name));
            if (!res.Succeeded) return BadRequest(res.Errors);

            var created = await _roleManager.FindByNameAsync(name);
            return CreatedAtAction(nameof(Get), new { id = created!.Id }, new { created.Id, created.Name });
        }

        // DELETE /api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null) return NotFound();

            // Blockiere das Löschen, wenn Benutzer die Rolle noch haben (liefere 409)
            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (users.Count > 0)
                return Conflict("Role is assigned to users.");

            var res = await _roleManager.DeleteAsync(role);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return NoContent();
        }
    }
}

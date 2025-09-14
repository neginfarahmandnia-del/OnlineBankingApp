using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankingApp.API.Models;
using OnlineBankingApp.Infrastructure.Persistence;
using OnlineBankingApp.Shared.Dtos;
using OnlineBankingApp.Domain.Entities;
namespace OnlineBankingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbteilungenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AbteilungenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/abteilungen
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AbteilungDto>>> GetAbteilungen()
        {
            return await _context.Abteilungen
                .Select(a => new AbteilungDto
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();
        }

        // GET: api/abteilungen/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AbteilungDto>> GetAbteilung(int id)
        {
            var abt = await _context.Abteilungen.FindAsync(id);
            if (abt == null) return NotFound();

            return new AbteilungDto
            {
                Id = abt.Id,
                Name = abt.Name
            };
        }

        // POST: api/abteilungen
        [HttpPost]
        public async Task<ActionResult<AbteilungDto>> CreateAbteilung(AbteilungDto dto)
        {
            var abt = new Abteilung
            {
                Name = dto.Name
            };

            _context.Abteilungen.Add(abt);
            await _context.SaveChangesAsync();

            dto.Id = abt.Id;
            return CreatedAtAction(nameof(GetAbteilung), new { id = abt.Id }, dto);
        }

        // PUT: api/abteilungen/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAbteilung(int id, AbteilungDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var abt = await _context.Abteilungen.FindAsync(id);
            if (abt == null) return NotFound();

            abt.Name = dto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/abteilungen/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAbteilung(int id)
        {
            var abt = await _context.Abteilungen.FindAsync(id);
            if (abt == null) return NotFound();

            _context.Abteilungen.Remove(abt);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

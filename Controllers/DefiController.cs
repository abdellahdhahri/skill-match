using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillMatchAPI.Data;
using SkillMatchAPI.Models;
using System.Data;

namespace SkillMatchAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DefiController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Defi>> GetDefi(int id)
        {
            var defi = await _context.Defis.FindAsync(id);

            if (defi == null)
            {
                return NotFound();
            }

            return defi;
        }

        [HttpPost]
        public async Task<ActionResult<Defi>> PostDefi(Defi defi)
        {
            defi.DateCreation = DateTime.Now;
            _context.Defis.Add(defi);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDefi), new { id = defi.Id }, defi);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDefi(int id, Defi defi)
        {
            if (id != defi.Id)
            {
                return BadRequest();
            }

            _context.Entry(defi).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Defis.Any(d => d.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDefi(int id)
        {
            var defi = await _context.Defis.FindAsync(id);
            if (defi == null)
            {
                return NotFound();
            }

            _context.Defis.Remove(defi);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

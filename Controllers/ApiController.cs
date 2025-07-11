using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;

namespace StudentFreelance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("skills-by-category/{categoryId}")]
        public async Task<IActionResult> GetSkillsByCategory(int categoryId)
        {
            var skills = await _context.Skills
                .Where(s => s.CategoryID == categoryId && s.IsActive)
                .Select(s => new { s.SkillID, s.SkillName })
                .ToListAsync();

            return Ok(skills);
        }
    }
} 
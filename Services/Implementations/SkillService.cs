using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace StudentFreelance.Services.Implementations
{
    public class SkillService : ISkillService
    {
        private readonly ApplicationDbContext _context;

        public SkillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SkillWithCategoryViewModel>> GetActiveSkillsWithCategoriesAsync()
        {
            return await _context.Skills
                .Where(s => s.IsActive)
                .Include(s => s.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(s =>
                    s.Category != null &&
                    s.Category.IsActive &&
                    s.Category.CategoryType == "SKILL" &&
                    (s.Category.ParentCategory == null || s.Category.ParentCategory.IsActive)
                )
                .Select(s => new SkillWithCategoryViewModel
                {
                    SkillID = s.SkillID,
                    SkillName = s.SkillName,
                    SubCategory = s.Category.CategoryName,
                    ParentCategory = s.Category.ParentCategory != null
                        ? s.Category.ParentCategory.CategoryName
                        : null
                })
                .ToListAsync();
        }

        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.SkillID == id);
        }

        public async Task CreateAsync(Skill skill)
        {
            skill.IsActive = true;
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Skill skill)
        {
            _context.Update(skill);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                skill.IsActive = false;
                await UpdateAsync(skill);
            }
        }

        public async Task UnhideAsync(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                skill.IsActive = true;
                await UpdateAsync(skill);
            }
        }
    }

}

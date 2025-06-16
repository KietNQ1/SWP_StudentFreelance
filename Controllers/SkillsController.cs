using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;

public class SkillsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SkillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Skills
    public async Task<IActionResult> Index()
    {
        var skills = await _context.Skills
            .Include(s => s.Category)
            .Where(s => s.IsActive)
            .ToListAsync();
        return View(skills);
    }

    // GET: Skills/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var skill = await _context.Skills
            .Include(s => s.Category)
            .FirstOrDefaultAsync(m => m.SkillID == id && m.IsActive);
        if (skill == null) return NotFound();

        return View(skill);
    }

    // GET: Skills/Create
    public IActionResult Create()
    {
        ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");
        return View();
    }

    // POST: Skills/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SkillName,CategoryID")] Skill skill)
    {
        if (ModelState.IsValid)
        {
            _context.Add(skill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", skill.CategoryID);
        return View(skill);
    }

    // GET: Skills/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var skill = await _context.Skills.FindAsync(id);
        if (skill == null || !skill.IsActive) return NotFound();

        ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", skill.CategoryID);
        return View(skill);
    }

    // POST: Skills/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SkillID,SkillName,CategoryID,IsActive")] Skill skill)
    {
        if (id != skill.SkillID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(skill);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkillExists(skill.SkillID)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", skill.CategoryID);
        return View(skill);
    }

    // GET: Skills/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var skill = await _context.Skills
            .Include(s => s.Category)
            .FirstOrDefaultAsync(m => m.SkillID == id && m.IsActive);
        if (skill == null) return NotFound();

        return View(skill);
    }

    // POST: Skills/Delete/5 (soft delete)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill != null)
        {
            skill.IsActive = false;
            _context.Update(skill);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool SkillExists(int id)
    {
        return _context.Skills.Any(e => e.SkillID == id);
    }
}

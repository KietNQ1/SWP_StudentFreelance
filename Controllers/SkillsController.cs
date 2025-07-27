using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using Microsoft.AspNetCore.Authorization;

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
        IQueryable<Skill> skillsQuery = _context.Skills.Include(s => s.Category);

        // Nếu không phải Admin/Moderator, chỉ thấy kỹ năng đang hoạt động
        if (!User.IsInRole("Admin") && !User.IsInRole("Moderator"))
        {
            skillsQuery = skillsQuery.Where(s => s.IsActive);
        }

        var skills = await skillsQuery.ToListAsync();
        return View(skills);
    }

    // GET: Skills/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var skill = await _context.Skills
            .Include(s => s.Category)
            .FirstOrDefaultAsync(m => m.SkillID == id);

        // Nếu kỹ năng bị ẩn thì chỉ admin/mod xem được
        if (skill == null || (!skill.IsActive && !User.IsInRole("Admin") && !User.IsInRole("Moderator")))
        {
            return NotFound();
        }

        return View(skill);
    }

    // GET: Skills/Create
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["CategoryID"] = new SelectList(
            _context.Categories.Where(c => c.IsActive),
            "CategoryID", "CategoryName");
        return View();
    }

    // POST: Skills/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SkillName,CategoryID")] Skill skill)
    {
        if (ModelState.IsValid)
        {
            skill.IsActive = true; // mặc định luôn kích hoạt
            _context.Add(skill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryID"] = new SelectList(
            _context.Categories.Where(c => c.IsActive),
            "CategoryID", "CategoryName", skill.CategoryID);
        return View(skill);
    }

    // GET: Skills/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {

        if (id == null) return NotFound();

        var skill = await _context.Skills.FindAsync(id);
        if (skill == null || (!skill.IsActive && !User.IsInRole("Admin") && !User.IsInRole("Moderator")))
        {
            return NotFound();
        }

        ViewData["CategoryID"] = new SelectList(
            _context.Categories.Where(c => c.IsActive),
            "CategoryID", "CategoryName", skill.CategoryID);
        return View(skill);
    }

    // POST: Skills/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Skill skill)
    {

        if (id != skill.SkillID)
        {
            // Nếu id không khớp ⇒ chắc chắn sẽ bị return
            return NotFound();
        }




        var existing = await _context.Skills.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.SkillName = skill.SkillName;
        existing.CategoryID = skill.CategoryID;
        existing.IsActive = skill.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    // GET: Skills/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var skill = await _context.Skills
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.SkillID == id && m.IsActive);

        if (skill == null) return NotFound();

        return View(skill);
    }

    // POST: Skills/DeleteConfirmed/5 => Ẩn kỹ năng
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

    // GET: Skills/Unhide/5
    [HttpGet]
    public async Task<IActionResult> Unhide(int? id)
    {
        if (id == null) return NotFound();

        var skill = await _context.Skills
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.SkillID == id && !s.IsActive);

        if (skill == null) return NotFound();

        return View(skill);
    }

    // POST: Skills/Unhide/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unhide(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill != null && !skill.IsActive)
        {
            skill.IsActive = true;
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

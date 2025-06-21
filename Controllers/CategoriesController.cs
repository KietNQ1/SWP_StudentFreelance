using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;

namespace StudentFreelance.Controllers
{
    [Authorize] // Bất kỳ action nào cũng require login
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context)
            => _context = context;

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            // Admin/Moderator thấy cả active & hidden
            IQueryable<Category> q = _context.Categories.Include(c => c.ParentCategory);
            if (!(User.IsInRole("Admin") || User.IsInRole("Moderator")))
            {
                // User thường chỉ thấy active
                q = q.Where(c => c.IsActive);
            }
            var list = await q.ToListAsync();
            return View(list);
        }

        // Hiển thị form tạo mới
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            ViewData["ParentCategoryID"] = GetCategorySelectList(null, null);
            return View(new Category());
        }

        [HttpPost, Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (category.CategoryType == "field")
                category.ParentCategoryID = null;
            else if (category.CategoryType == "skill" && category.ParentCategoryID == null)
                ModelState.AddModelError("ParentCategoryID", "Phải chọn danh mục cha.");

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryID"] = GetCategorySelectList(null, category.CategoryType);
            return View(category);
        }

        // GET: /Categories/Edit/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var c = await _context.Categories.IgnoreQueryFilters()
                              .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c == null) return NotFound();

            bool isParent = await _context.Categories.AnyAsync(x => x.ParentCategoryID == id);
            bool isChild = c.ParentCategoryID != null && !isParent;
            ViewBag.IsParent = isParent;
            ViewBag.IsChild = isChild;
            ViewData["ParentCategoryID"] = GetCategorySelectList(c.CategoryID, c.CategoryType);
            return View(c);
        }

        [HttpPost, Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("CategoryID,CategoryName,CategoryType,ParentCategoryID,Description")] Category category,
            string confirm)
        {
            if (id != category.CategoryID) return NotFound();

            var original = await _context.Categories.IgnoreQueryFilters()
                                     .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (original == null) return NotFound();

            bool isParent = await _context.Categories.AnyAsync(x => x.ParentCategoryID == id);
            bool isChild = original.ParentCategoryID != null && !isParent;

            // confirm flow omitted for brevity...
            // sau confirm thì gán và SaveChanges như bạn đã có

            // … phần Update như cũ …

            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Hide/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Hide(int? id)
        {
            if (id == null) return NotFound();
            var c = await _context.Categories.IgnoreQueryFilters()
                               .Include(x => x.ParentCategory)
                               .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c == null) return NotFound();
            return View(c);  // Views/Categories/Hide.cshtml
        }

        // POST /Categories/Hide/5
        [HttpPost, ActionName("Hide"), Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideConfirmed(int id)
        {
            var c = await _context.Categories.IgnoreQueryFilters()
                               .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c != null)
            {
                c.IsActive = false;
                _context.Update(c);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // **MỚI**: GET /Categories/Unhide/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Unhide(int? id)
        {
            if (id == null) return NotFound();
            var c = await _context.Categories.IgnoreQueryFilters()
                               .Include(x => x.ParentCategory)
                               .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c == null) return NotFound();
            return View(c);  // Views/Categories/Unhide.cshtml
        }

        // **MỚI**: POST /Categories/Unhide/5
        [HttpPost, ActionName("Unhide"), Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnhideConfirmed(int id)
        {
            var c = await _context.Categories.IgnoreQueryFilters()
                               .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c != null)
            {
                c.IsActive = true;
                _context.Update(c);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper tạo select list
        private List<SelectListItem> GetCategorySelectList(int? currentId, string? type)
        {
            var all = _context.Categories
                       .IgnoreQueryFilters()
                       .ToList();
            // Loại bỏ current để không tự làm parent
            var parents = all.Where(x => x.CategoryID != currentId).ToList();
            if (type == "skill")
                parents = parents.Where(x => x.ParentCategoryID == null).ToList();
            else if (type == "field")
                parents = new List<Category>();
            return parents.Select(x => new SelectListItem
            {
                Value = x.CategoryID.ToString(),
                Text = x.CategoryName + (x.IsActive ? "" : " (ẩn)")
            }).ToList();
        }
    }
}

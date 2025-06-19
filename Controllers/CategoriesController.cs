// CategoriesController.cs - Đầy đủ CRUD + Hide + Razor logic phù hợp (Không cho sửa Name, Type và Danh mục cha nếu là danh mục cha)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;

namespace StudentFreelance.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = (User.IsInRole("Admin") || User.IsInRole("Moderator"))
                ? await _context.Categories.IgnoreQueryFilters().Include(c => c.ParentCategory).ToListAsync()
                : await _context.Categories.Include(c => c.ParentCategory).ToListAsync();

            return View(categories);
        }

        private List<SelectListItem> GetCategorySelectList(int? currentCategoryId = null, string? type = null)
        {
            var allCategories = (User.IsInRole("Admin") || User.IsInRole("Moderator"))
                ? _context.Categories.IgnoreQueryFilters().ToList()
                : _context.Categories.ToList();

            var parentCandidates = allCategories
                .Where(c => c.CategoryID != currentCategoryId)
                .ToList();

            if (type == "skill")
            {
                parentCandidates = parentCandidates
                    .Where(c => c.ParentCategoryID == null)
                    .ToList();
            }
            else if (type == "field")
            {
                parentCandidates = new List<Category>(); // Không có danh mục cha nào được chọn nếu là field
            }

            return parentCandidates.Select(c => new SelectListItem
            {
                Value = c.CategoryID.ToString(),
                Text = c.CategoryName + (c.IsActive ? "" : " (ẩn)")
            }).ToList();
        }

        [HttpGet]
        public IActionResult GetParentCategories(string type, int? currentCategoryId)
        {
            var list = GetCategorySelectList(currentCategoryId, type);
            return Json(list);
        }

        public IActionResult Create()
        {
            ViewData["ParentCategoryID"] = GetCategorySelectList(null, null);
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (category.CategoryType == "field")
            {
                category.ParentCategoryID = null;
            }
            else if (category.CategoryType == "skill" && category.ParentCategoryID == null)
            {
                ModelState.AddModelError("ParentCategoryID", "Danh mục con bắt buộc phải chọn danh mục cha.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ParentCategoryID"] = GetCategorySelectList(null, category.CategoryType);
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) return NotFound();

            bool isParent = await _context.Categories.AnyAsync(c => c.ParentCategoryID == id);
            bool isChild = category.ParentCategoryID != null && !isParent;

            ViewBag.IsParent = isParent;
            ViewBag.IsChild = isChild;
            ViewData["ParentCategoryID"] = GetCategorySelectList(category.CategoryID, category.CategoryType);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,Description,CategoryType,ParentCategoryID,CategoryName")] Category category, string confirm)
        {
            if (id != category.CategoryID) return NotFound();

            var original = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryID == id);
            if (original == null) return NotFound();

            bool isParent = await _context.Categories.AnyAsync(c => c.ParentCategoryID == id);
            bool isChild = original.ParentCategoryID != null && !isParent;

            if (confirm != "yes")
            {
                TempData["ShowConfirm"] = true;
                TempData["EditData"] = System.Text.Json.JsonSerializer.Serialize(category);
                ViewBag.IsParent = isParent;
                ViewBag.IsChild = isChild;
                ViewData["ParentCategoryID"] = GetCategorySelectList(category.CategoryID, category.CategoryType);
                return View(category);
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(category.Description))
            {
                ModelState.AddModelError("Description", "Mô tả không được để trống.");
            }

            if (ModelState.IsValid)
            {
                original.Description = category.Description;
                original.CategoryName = category.CategoryName;

                if (!isParent)
                {
                    original.CategoryType = category.CategoryType;

                    if (category.CategoryType == "field")
                    {
                        original.ParentCategoryID = null;
                    }
                    else if (category.CategoryType == "skill")
                    {
                        if (category.ParentCategoryID == null)
                        {
                            ModelState.AddModelError("ParentCategoryID", "Danh mục con bắt buộc phải chọn danh mục cha.");
                        }
                        else
                        {
                            original.ParentCategoryID = category.ParentCategoryID;
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Categories.Update(original);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.IsParent = isParent;
            ViewBag.IsChild = isChild;
            ViewData["ParentCategoryID"] = GetCategorySelectList(category.CategoryID, category.CategoryType);
            return View(category);
        }


        public async Task<IActionResult> Hide(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Hide")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideConfirmed(int id)
        {
            var category = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category != null)
            {
                category.IsActive = false;
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }
    }
}

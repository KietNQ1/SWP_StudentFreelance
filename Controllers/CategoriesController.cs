using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using System.IO;

namespace StudentFreelance.Controllers
{
    [Authorize] // Bất kỳ action nào cũng require login
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ParentCategoryID"] = GetCategorySelectList(null, null);
            
            // Lấy danh sách các ảnh trong thư mục Icon
            var iconPath = Path.Combine(_hostEnvironment.WebRootPath, "image", "Icon");
            var icons = Directory.Exists(iconPath) 
                ? Directory.GetFiles(iconPath).Select(path => Path.GetFileName(path))
                : new List<string>();
            
            ViewBag.Icons = icons.Select(icon => new SelectListItem
            {
                Value = $"/image/Icon/{icon}",
                Text = icon
            }).ToList();
            
            return View(new Category());
        }

        [HttpPost, Authorize(Roles = "Admin")]
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
            
            // Lấy lại danh sách ảnh nếu validation thất bại
            var iconPath = Path.Combine(_hostEnvironment.WebRootPath, "image", "Icon");
            var icons = Directory.Exists(iconPath) 
                ? Directory.GetFiles(iconPath).Select(path => Path.GetFileName(path))
                : new List<string>();
            
            ViewBag.Icons = icons.Select(icon => new SelectListItem
            {
                Value = $"/image/Icon/{icon}",
                Text = icon
            }).ToList();
            
            return View(category);
        }

        // GET: /Categories/Edit/5
        [Authorize(Roles = "Admin")]
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
            
            // Lấy danh sách các ảnh trong thư mục Icon
            var iconPath = Path.Combine(_hostEnvironment.WebRootPath, "image", "Icon");
            var icons = Directory.Exists(iconPath) 
                ? Directory.GetFiles(iconPath).Select(path => Path.GetFileName(path))
                : new List<string>();
            
            ViewBag.Icons = icons.Select(icon => new SelectListItem
            {
                Value = $"/image/Icon/{icon}",
                Text = icon,
                Selected = $"/image/Icon/{icon}" == c.ImagePath
            }).ToList();
            
            return View(c);
        }
        
        [HttpPost, Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("CategoryID,CategoryName,CategoryType,ParentCategoryID,Description,ImagePath")] Category category,
            string confirm)
        {
            if (id != category.CategoryID) return NotFound();

            var original = await _context.Categories.IgnoreQueryFilters()
                                         .FirstOrDefaultAsync(x => x.CategoryID == id);
            if (original == null) return NotFound();

            // ✅ Thêm đoạn cập nhật giá trị:
            original.CategoryName = category.CategoryName;
            original.Description = category.Description;
            original.ImagePath = category.ImagePath;

            // ✅ Chỉ cho sửa CategoryType/Parent nếu là child (Skill)
            bool isParent = await _context.Categories.AnyAsync(x => x.ParentCategoryID == id);
            bool isChild = original.ParentCategoryID != null && !isParent;

            if (isChild)
            {
                original.CategoryType = category.CategoryType;
                original.ParentCategoryID = category.CategoryType == "skill" ? category.ParentCategoryID : null;
            }

            await _context.SaveChangesAsync(); // Lưu thay đổi

            return RedirectToAction(nameof(Index));
        }


        // GET: /Categories/Hide/5
        [Authorize(Roles = "Admin")]
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
        [HttpPost, ActionName("Hide"), Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [HttpPost, ActionName("Unhide"), Authorize(Roles = "Admin")]
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


        // GET: /Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound();

            // Chặn người dùng thường xem danh mục đã bị ẩn
            if (!category.IsActive && !(User.IsInRole("Admin") || User.IsInRole("Moderator")))
                return NotFound();

            return View(category); // sẽ render Views/Categories/Details.cshtml
        }

        // GET: /Categories/GetIcons
        [Authorize(Roles = "Admin")]
        public IActionResult GetIcons()
        {
            var iconPath = Path.Combine(_hostEnvironment.WebRootPath, "image", "Icon");
            if (!Directory.Exists(iconPath))
            {
                return Json(new List<string>());
            }
            
            var icons = Directory.GetFiles(iconPath)
                .Select(path => new {
                    path = $"/image/Icon/{Path.GetFileName(path)}",
                    name = Path.GetFileName(path)
                })
                .ToList();
                
            return Json(icons);
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

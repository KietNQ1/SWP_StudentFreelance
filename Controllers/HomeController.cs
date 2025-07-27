    using System.Diagnostics;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using StudentFreelance.DbContext;
    using StudentFreelance.Models;
    using StudentFreelance.Services.Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;

namespace StudentFreelance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ILocationApiService _locationApiService;
        private readonly IAdvertisementService _advertisementService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context,
            ILocationApiService locationApiService,
            IAdvertisementService advertisementService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _logger = logger;
            _context = context;
            _locationApiService = locationApiService;
            _advertisementService = advertisementService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Clean up expired advertisements
                await _advertisementService.CleanupExpiredAdvertisementsAsync();

                // Get active advertisements for the slideshow
                var sortedAds = await _advertisementService.GetActiveAdvertisementsAsync();
                
                // Sắp xếp quảng cáo theo loại gói và ngày bắt đầu
                sortedAds = sortedAds
                    .OrderByDescending(a => a.PackageType.PackageTypeName == "Featured") // Featured ads first
                    .ThenByDescending(a => a.StartDate)  // Newer ads first within each type
                    .ToList();

                _logger.LogInformation("Loaded {0} active advertisements for slideshow", sortedAds.Count);

                // Pass advertisements to the view
                ViewBag.Advertisements = sortedAds;

                // Lấy danh sách các category cha (ParentCategoryID = null) và có CategoryType = "PROJECT"
                var parentCategories = await _context.Categories
                    .Where(c => c.ParentCategoryID == null && c.CategoryType == "Field" && c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();
                
                ViewBag.ParentCategories = parentCategories;

                // Lấy danh sách tất cả kỹ năng cho dropdown
                ViewBag.AllSkills = await _context.Skills
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                // Lấy danh sách tất cả tỉnh/thành phố từ API cho dropdown
                var apiProvinces = await _locationApiService.GetProvincesAsync();
                ViewBag.AllProvinces = apiProvinces.Select(p => new
                {
                    ProvinceID = int.Parse(p.Id),
                    Name = p.Name,
                    Code = p.Slug ?? p.Id
                }).OrderBy(p => p.Name).ToList();

                List<Project> topProjects;

                var baseQuery = _context.Projects
                    .Where(p => p.IsActive && p.StatusID == 1)
                    .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(ps => ps.Skill)
                    .Include(p => p.Business)
                    .Include(p => p.Address)
                    .Include(p => p.Category)
                    .Include(p => p.Type)
                    .Include(p => p.Status);

                if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Student"))
                {
                    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        // Lấy danh sách skillID mà sinh viên đang có
                        var studentSkillIds = await _context.StudentSkills
                            .Where(ss => ss.UserID == userId)
                            .Select(ss => ss.SkillID)
                            .ToListAsync();

                        // Ưu tiên dự án có kỹ năng trùng
                        topProjects = baseQuery
                            .AsEnumerable()
                            .OrderByDescending(p => p.ProjectSkillsRequired.Any(ps => studentSkillIds.Contains(ps.SkillID))) // true trước
                            .ThenByDescending(p => p.CreatedAt)
                            .Take(6)
                            .ToList();
                    }
                    else
                    {
                        topProjects = await baseQuery.OrderByDescending(p => p.CreatedAt).Take(6).ToListAsync();
                    }
                }
                else
                {
                    topProjects = await baseQuery.OrderByDescending(p => p.CreatedAt).Take(6).ToListAsync();
                }

                ViewBag.TopProjects = topProjects;

                // Get popular businesses, prioritizing VIP businesses
                var businessRoleId = _roleManager.Roles.FirstOrDefault(r => r.Name == "Business")?.Id;
                if (businessRoleId.HasValue)
                {
                    // Get all business users
                    var businessUsers = await _context.UserRoles
                        .Where(ur => ur.RoleId == businessRoleId.Value)
                        .Join(_context.Users,
                            ur => ur.UserId,
                            u => u.Id,
                            (ur, u) => u)
                        .Where(u => u.IsActive)
                        .ToListAsync();

                    // Sort businesses: VIP first, then by VIP subscription date (earliest first)
                    var popularBusinesses = businessUsers
                        .OrderByDescending(b => b.VipStatus) // VIP users first
                        .ThenBy(b => b.VipStatus ? b.VipExpiryDate : null) // Earlier subscription first (will expire sooner)
                        .Take(6)
                        .ToList();

                    ViewBag.PopularBusinesses = popularBusinesses;
                }
                else
                {
                    ViewBag.PopularBusinesses = new List<ApplicationUser>();
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage with advertisements");

                // Fallback to no advertisements in case of error
                ViewBag.Advertisements = new List<StudentFreelance.Models.Advertisement>();
                ViewBag.PopularBusinesses = new List<ApplicationUser>();

                // Still load other data
                ViewBag.AllSkills = await _context.Skills
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                var apiProvinces = await _locationApiService.GetProvincesAsync();
                ViewBag.AllProvinces = apiProvinces.Select(p => new
                {
                    ProvinceID = int.Parse(p.Id),
                    Name = p.Name,
                    Code = p.Slug ?? p.Id
                }).OrderBy(p => p.Name).ToList();

                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

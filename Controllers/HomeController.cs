    using System.Diagnostics;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using StudentFreelance.DbContext;
    using StudentFreelance.Models;
    using StudentFreelance.Services.Interfaces;

namespace StudentFreelance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ILocationApiService _locationApiService;
        private readonly IAdvertisementService _advertisementService;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context,
            ILocationApiService locationApiService,
            IAdvertisementService advertisementService)
        {
            _logger = logger;
            _context = context;
            _locationApiService = locationApiService;
            _advertisementService = advertisementService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Clean up expired advertisements
                await _advertisementService.CleanupExpiredAdvertisementsAsync();

                // Get active advertisements for the slideshow
                var activeAds = await _advertisementService.GetActiveAdvertisementsAsync();

                // Sort advertisements by package type (Featured first) and then by start date
                var sortedAds = activeAds
                    .OrderBy(a => a.PackageTypeID == 2 ? 0 : 1)  // Featured ads first (PackageTypeID 2 is Featured)
                    .ThenByDescending(a => a.StartDate)  // Newer ads first within each type
                    .ToList();

                _logger.LogInformation("Loaded {0} active advertisements for slideshow", sortedAds.Count);

                // Pass advertisements to the view
                ViewBag.Advertisements = sortedAds;

                // Lấy danh sách tất cả kỹ năng cho dropdown
                ViewBag.AllSkills = await _context.Skills
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                // Lấy danh sách tất cả tỉnh/thành phố từ API cho dropdown
                var apiProvinces = await _locationApiService.GetProvincesAsync();
                ViewBag.AllProvinces = apiProvinces.Select(p => new Province
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


                ViewBag.TopProjects = topProjects;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage with advertisements");

                // Fallback to no advertisements in case of error
                ViewBag.Advertisements = new List<StudentFreelance.Models.Advertisement>();

                // Still load other data
                ViewBag.AllSkills = await _context.Skills
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                var apiProvinces = await _locationApiService.GetProvincesAsync();
                ViewBag.AllProvinces = apiProvinces.Select(p => new Province
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

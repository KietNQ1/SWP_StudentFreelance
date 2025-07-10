using System.Diagnostics;
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

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context,
            ILocationApiService locationApiService)
        {
            _logger = logger;
            _context = context;
            _locationApiService = locationApiService;
        }

        public async Task<IActionResult> Index()
        {
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

            return View();
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public SearchController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Method to get provinces for dropdown
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .ToListAsync();
                
            return PartialView("_ProvinceDropdown", provinces);
        }

        public async Task<IActionResult> SearchJob(string query, string location, int? categoryId, List<int> skillIds, int? userId, int? provinceId)
        {
            // Lấy danh sách dự án từ model Project
            var projects = _context.Projects
                .Include(p => p.Business)
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Address)
                    .ThenInclude(a => a.Province)
                .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(ps => ps.Skill)
                .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(ps => ps.ImportanceLevel)
                .Where(p => p.IsActive);

            // Tìm kiếm theo từ khóa trong tiêu đề hoặc mô tả
            if (!string.IsNullOrEmpty(query))
            {
                projects = projects.Where(p => 
                    p.Title.Contains(query) || 
                    p.Description.Contains(query));
            }

            // Tìm kiếm theo địa điểm (location)
            if (!string.IsNullOrEmpty(location))
            {
                // Tìm các tỉnh/thành phố phù hợp với từ khóa location
                var matchingProvinces = await _context.Provinces
                    .Where(p => p.Name.Contains(location))
                    .Select(p => p.ProvinceID)
                    .ToListAsync();

                if (matchingProvinces.Any())
                {
                    projects = projects.Where(p => 
                        p.Address != null && 
                        matchingProvinces.Contains(p.Address.ProvinceID.Value));
                }
            }

            // Lọc theo danh mục dựa trên model Category
            if (categoryId.HasValue)
            {
                // Tìm danh mục theo CategoryID từ model Category
                var selectedCategory = await _context.Categories.FindAsync(categoryId.Value);
                if (selectedCategory != null)
                {
                    if (selectedCategory.CategoryType == "Field")
                    {
                        // Nếu là danh mục chính, lấy cả dự án thuộc danh mục con
                        var subCategoryIds = await _context.Categories
                            .Where(c => c.ParentCategoryID == selectedCategory.CategoryID && c.IsActive)
                            .Select(c => c.CategoryID)
                            .ToListAsync();

                        projects = projects.Where(p => 
                            p.CategoryID == categoryId.Value || 
                            subCategoryIds.Contains(p.CategoryID));
                    }
                    else
                    {
                        // Nếu là danh mục con, chỉ lấy dự án thuộc danh mục đó
                        projects = projects.Where(p => p.CategoryID == categoryId.Value);
                    }
                }
            }

            // Lọc theo kỹ năng dựa trên model Skill và ProjectSkillRequired
            if (skillIds != null && skillIds.Any())
            {
                projects = projects.Where(p => 
                    p.ProjectSkillsRequired.Any(ps => skillIds.Contains(ps.SkillID) && ps.IsActive));
            }

            // Lọc theo người đăng (business) dựa trên model User
            if (userId.HasValue)
            {
                projects = projects.Where(p => p.BusinessID == userId.Value);
            }
            
            // Lọc theo tỉnh/thành phố
            if (provinceId.HasValue)
            {
                projects = projects.Where(p => 
                    p.Address != null && 
                    p.Address.ProvinceID == provinceId.Value);
            }

            // Lấy danh sách danh mục từ model Category
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryType)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            // Lấy danh sách kỹ năng từ model Skill
            ViewBag.Skills = await _context.Skills
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category.CategoryName)
                .ThenBy(s => s.SkillName)
                .ToListAsync();
            
            // Lấy danh sách người dùng doanh nghiệp
            var businessUsers = await _userManager.GetUsersInRoleAsync("Business");
            ViewBag.BusinessUsers = businessUsers.Where(u => u.IsActive).OrderBy(u => u.CompanyName).ToList();

            // Lấy danh sách tỉnh/thành phố
            ViewBag.Provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Lấy các mức độ quan trọng từ model ImportanceLevel
            ViewBag.ImportanceLevels = await _context.ImportanceLevels
                .Where(l => l.IsActive)
                .ToListAsync();

            // Lấy các mức độ thành thạo từ model ProficiencyLevel
            ViewBag.ProficiencyLevels = await _context.ProficiencyLevels
                .Where(l => l.IsActive)
                .ToListAsync();

            ViewBag.Query = query;
            ViewBag.Location = location;
            ViewBag.CategoryId = categoryId;
            ViewBag.SkillIds = skillIds;
            ViewBag.UserId = userId;
            ViewBag.ProvinceId = provinceId;

            return View(await projects.ToListAsync());
        }

        public async Task<IActionResult> SearchStudents(string query, List<int> skillIds, int? provinceId)
        {
            // Lấy danh sách sinh viên
            var allStudents = await _userManager.GetUsersInRoleAsync("Student");
            var students = allStudents.Where(u => u.IsActive).AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(query))
            {
                students = students.Where(s => 
                    s.FullName.Contains(query) || 
                    s.Email.Contains(query) || 
                    s.UserName.Contains(query) || 
                    (s.University != null && s.University.Contains(query)) || 
                    (s.Major != null && s.Major.Contains(query)))
                    .AsQueryable();
            }

            // Lọc theo kỹ năng
            if (skillIds != null && skillIds.Any())
            {
                var studentIdsWithSkills = await _context.StudentSkills
                    .Where(ss => skillIds.Contains(ss.SkillID) && ss.IsActive)
                    .Select(ss => ss.UserID)
                    .ToListAsync();
                
                students = students.Where(s => studentIdsWithSkills.Contains(s.Id)).AsQueryable();
            }

            // Lọc theo tỉnh/thành phố
            if (provinceId.HasValue)
            {
                var studentIdsInProvince = await _context.Users
                    .Where(u => u.AddressID.HasValue)
                    .Join(_context.Addresses.Where(a => a.ProvinceID == provinceId.Value),
                        u => u.AddressID,
                        a => a.AddressID,
                        (u, a) => u.Id)
                    .ToListAsync();
                
                students = students.Where(s => studentIdsInProvince.Contains(s.Id)).AsQueryable();
            }

            // Lấy danh sách kỹ năng
            ViewBag.Skills = await _context.Skills
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category.CategoryName)
                .ThenBy(s => s.SkillName)
                .ToListAsync();

            // Lấy danh sách tỉnh/thành phố
            ViewBag.Provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Lấy danh sách kỹ năng của từng sinh viên
            var studentSkills = await _context.StudentSkills
                .Include(ss => ss.Skill)
                .Include(ss => ss.ProficiencyLevel)
                .Where(ss => ss.IsActive)
                .ToListAsync();

            // Chuyển đổi từ UserID sang Id nếu cần thiết
            var studentSkillsDict = studentSkills
                .GroupBy(ss => ss.UserID)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.StudentSkills = studentSkillsDict;

            ViewBag.Query = query;
            ViewBag.SkillIds = skillIds;
            ViewBag.ProvinceId = provinceId;

            return View(students.ToList());
        }

        public async Task<IActionResult> SearchBusinesses(string query, int? provinceId)
        {
            // Lấy danh sách doanh nghiệp
            var allBusinesses = await _userManager.GetUsersInRoleAsync("Business");
            var businesses = allBusinesses.Where(u => u.IsActive).AsQueryable();

            // Tìm kiếm theo tên công ty hoặc lĩnh vực
            if (!string.IsNullOrEmpty(query))
            {
                businesses = businesses.Where(b => 
                    (b.CompanyName != null && b.CompanyName.Contains(query)) || 
                    (b.Industry != null && b.Industry.Contains(query)))
                    .AsQueryable();
            }

            // Lọc theo tỉnh/thành phố
            if (provinceId.HasValue)
            {
                var businessIdsInProvince = await _context.Users
                    .Where(u => u.AddressID.HasValue)
                    .Join(_context.Addresses.Where(a => a.ProvinceID == provinceId.Value),
                        u => u.AddressID,
                        a => a.AddressID,
                        (u, a) => u.Id)
                    .ToListAsync();
                
                businesses = businesses.Where(b => businessIdsInProvince.Contains(b.Id)).AsQueryable();
            }

            // Lấy số lượng dự án của mỗi doanh nghiệp
            var projectCounts = await _context.Projects
                .Where(p => p.IsActive)
                .GroupBy(p => p.BusinessID)
                .Select(g => new { BusinessID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BusinessID, x => x.Count);

            // Chuyển đổi từ ID sang Id nếu cần thiết
            var businessIdToProjectCount = new Dictionary<int, int>();
            foreach (var business in businesses)
            {
                if (projectCounts.ContainsKey(business.Id))
                {
                    businessIdToProjectCount[business.Id] = projectCounts[business.Id];
                }
                else
                {
                    businessIdToProjectCount[business.Id] = 0;
                }
            }

            ViewBag.ProjectCounts = businessIdToProjectCount;

            // Lấy danh sách tỉnh/thành phố
            ViewBag.Provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Query = query;
            ViewBag.ProvinceId = provinceId;

            return View(businesses.ToList());
        }
    }
} 
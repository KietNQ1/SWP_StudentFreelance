using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using Microsoft.AspNetCore.Identity;
using StudentFreelance.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentFreelance.ViewModels;
using System;
using System.Security.Claims;

namespace StudentFreelance.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILocationApiService _locationApiService;

        public SearchController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILocationApiService locationApiService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _locationApiService = locationApiService;
        }

        // Method to get provinces for dropdown
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _locationApiService.GetProvincesAsync();
            
            // Convert API provinces to OptionItem for the view
            var provinceOptions = provinces.Select(p => new OptionItem
            {
                ID = p.Id,
                Name = p.Name
            }).OrderBy(p => p.Name).ToList();
                
            return PartialView("_ProvinceDropdown", provinceOptions);
        }

        public async Task<IActionResult> SearchJob(string query, string location, int? categoryId, List<int> skillIds, int? userId, string provinceCode)
        {
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
                if (int.TryParse(userIdStr, out int studentUserId))
                {
                    // Lấy danh sách skillID mà sinh viên đang có
                    var studentSkillIds = await _context.StudentSkills
                        .Where(ss => ss.UserID == studentUserId)
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

            Console.WriteLine($"[DEBUG] SearchJob called with provinceCode: {provinceCode}");
            
            // Lấy danh sách dự án
            var projects = _context.Projects
                .Include(p => p.Business)
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Address)
                .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(ps => ps.Skill)
                .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(ps => ps.ImportanceLevel)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Debug: Hiển thị thông tin địa chỉ của tất cả dự án
            Console.WriteLine($"[DEBUG] Total projects before filtering: {projects.Count()}");
            foreach (var project in projects.Take(10))
            {
                Console.WriteLine($"[DEBUG] Project: {project.Title}, Address: {project.Address?.ProvinceName ?? "NULL"}, ProvinceCode: {project.Address?.ProvinceCode ?? "NULL"}");
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(query))
            {
                string searchQueryLower = query.ToLower();
                projects = projects.Where(p => 
                    (p.Title != null && p.Title.ToLower().Contains(searchQueryLower)) || 
                    (p.Description != null && p.Description.ToLower().Contains(searchQueryLower)) || 
                    (p.Business != null && p.Business.CompanyName != null && p.Business.CompanyName.ToLower().Contains(searchQueryLower)));
            }

            // Tìm kiếm theo địa điểm
            if (!string.IsNullOrEmpty(location))
            {
                string locationLower = location.ToLower();
                projects = projects.Where(p => 
                    (p.Address != null && (
                        (p.Address.ProvinceName != null && p.Address.ProvinceName.ToLower().Contains(locationLower)) || 
                        (p.Address.DistrictName != null && p.Address.DistrictName.ToLower().Contains(locationLower)) || 
                        (p.Address.WardName != null && p.Address.WardName.ToLower().Contains(locationLower)) || 
                        (p.Address.DetailAddress != null && p.Address.DetailAddress.ToLower().Contains(locationLower))
                    )));
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                // Kiểm tra xem đây có phải là danh mục cha không
                var isParentCategory = await _context.Categories
                    .AnyAsync(c => c.ParentCategoryID == categoryId);
                
                if (isParentCategory)
                {
                    // Nếu là danh mục cha, lấy tất cả ID của danh mục con
                    var subCategoryIds = await _context.Categories
                        .Where(c => c.ParentCategoryID == categoryId)
                        .Select(c => c.CategoryID)
                        .ToListAsync();
                    
                    // Lọc theo danh mục con
                    projects = projects.Where(p => subCategoryIds.Contains(p.CategoryID));
                }
                else
                {
                    // Nếu không có danh mục con, chỉ tìm theo danh mục hiện tại
                    projects = projects.Where(p => p.CategoryID == categoryId);
                }
            }

            // Lọc theo kỹ năng
            if (skillIds != null && skillIds.Any())
            {
                projects = projects.Where(p => p.ProjectSkillsRequired.Any(ps => skillIds.Contains(ps.SkillID)));
            }

            // Lọc theo doanh nghiệp
            if (userId.HasValue)
            {
                projects = projects.Where(p => p.BusinessID == userId.Value);
            }
            
            // Lọc theo tỉnh/thành phố
            if (!string.IsNullOrEmpty(provinceCode))
            {
                Console.WriteLine($"[DEBUG] Filtering projects by provinceCode: {provinceCode}");
                
                // Lấy danh sách các dự án có địa chỉ
                var projectsWithAddress = projects.Where(p => p.Address != null).ToList();
                Console.WriteLine($"[DEBUG] Projects with address: {projectsWithAddress.Count}");
                
                // Kiểm tra từng dự án để tìm những dự án có ProvinceCode khớp
                var filteredProjects = projectsWithAddress.Where(p => 
                    p.Address != null && 
                    !string.IsNullOrEmpty(p.Address.ProvinceCode) &&
                    p.Address.ProvinceCode == provinceCode).ToList();
                
                Console.WriteLine($"[DEBUG] Projects matching provinceCode exactly: {filteredProjects.Count}");
                
                // Nếu không tìm thấy dự án nào khớp chính xác, thử tìm theo ProvinceName
                if (!filteredProjects.Any() && !string.IsNullOrEmpty(provinceCode))
                {
                    // Lấy tên tỉnh từ API để so sánh
                    var provinces = await _locationApiService.GetProvincesAsync();
                    var selectedProvince = provinces.FirstOrDefault(p => p.Id == provinceCode);
                    
                    if (selectedProvince != null)
                    {
                        Console.WriteLine($"[DEBUG] Selected province name: {selectedProvince.Name}");
                        
                        // Tìm dự án có ProvinceName khớp với tên tỉnh đã chọn
                        filteredProjects = projectsWithAddress.Where(p => 
                            p.Address != null && 
                            !string.IsNullOrEmpty(p.Address.ProvinceName) &&
                            p.Address.ProvinceName.Contains(selectedProvince.Name)).ToList();
                        
                        Console.WriteLine($"[DEBUG] Projects matching province name: {filteredProjects.Count}");
                    }
                }
                
                // Cập nhật danh sách dự án
                projects = filteredProjects.AsQueryable();
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

            // Lấy danh sách tỉnh/thành phố từ API
            var apiProvinces = await _locationApiService.GetProvincesAsync();
            ViewBag.Provinces = apiProvinces.Select(p => new
            {
                ID = p.Id,
                Name = p.Name
            }).OrderBy(p => p.Name).ToList();

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
            ViewBag.ProvinceCode = provinceCode;

            return View(projects.ToList());
        }

        public async Task<IActionResult> SearchStudents(string query, List<int> skillIds, string provinceCode)
        {
            // Lấy danh sách sinh viên
            var allStudents = await _userManager.GetUsersInRoleAsync("Student");
            var students = allStudents.Where(u => u.IsActive).AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(query))
            {
                string searchQueryLower = query.ToLower();
                students = students.Where(s => 
                    (s.FullName != null && s.FullName.ToLower().Contains(searchQueryLower)) || 
                    (s.Email != null && s.Email.ToLower().Contains(searchQueryLower)) || 
                    (s.UserName != null && s.UserName.ToLower().Contains(searchQueryLower)) || 
                    (s.University != null && s.University.ToLower().Contains(searchQueryLower)) || 
                    (s.Major != null && s.Major.ToLower().Contains(searchQueryLower)));
            }

            // Lọc theo kỹ năng
            if (skillIds != null && skillIds.Any())
            {
                var studentIdsWithSkills = await _context.StudentSkills
                    .Where(ss => skillIds.Contains(ss.SkillID) && ss.IsActive)
                    .Select(ss => ss.UserID)
                    .ToListAsync();

                students = students.Where(s => studentIdsWithSkills.Contains(s.Id));
            }

            // Lọc theo tỉnh/thành phố
            if (!string.IsNullOrEmpty(provinceCode))
            {
                // Lấy danh sách ID của sinh viên có địa chỉ ở tỉnh/thành phố đã chọn
                var studentIdsInProvince = await _context.Users
                    .Where(u => u.Address != null && u.Address.ProvinceCode == provinceCode)
                    .Select(u => u.Id)
                    .ToListAsync();

                students = students.Where(s => studentIdsInProvince.Contains(s.Id));
            }

            // Lấy danh sách kỹ năng từ model Skill
            var skills = await _context.Skills
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category.CategoryName)
                .ThenBy(s => s.SkillName)
                .ToListAsync();

            // Lấy danh sách danh mục
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // Lấy danh sách tỉnh/thành phố từ API
            var apiProvinces = await _locationApiService.GetProvincesAsync();
            var provinces = apiProvinces.Select(p => new
            {
                ID = p.Id,
                Name = p.Name
            }).OrderBy(p => p.Name).ToList();

            // Lấy thông tin kỹ năng của sinh viên
            var studentsList = students.ToList();
            var studentIds = studentsList.Select(s => s.Id).ToList();
            var allStudentSkills = await _context.StudentSkills
                .Where(ss => studentIds.Contains(ss.UserID) && ss.IsActive)
                .Include(ss => ss.Skill)
                .Include(ss => ss.ProficiencyLevel)
                .ToListAsync();

            // Tạo Dictionary để lưu trữ kỹ năng theo ID sinh viên
            var studentSkillsDict = new Dictionary<int, List<StudentSkill>>();
            foreach (var skill in allStudentSkills)
            {
                if (!studentSkillsDict.ContainsKey(skill.UserID))
                {
                    studentSkillsDict[skill.UserID] = new List<StudentSkill>();
                }
                studentSkillsDict[skill.UserID].Add(skill);
            }

            // Tạo view model
            var viewModel = new SearchStudentsViewModel
            {
                Students = studentsList,
                Skills = skills,
                Categories = categories,
                SelectedSkillIds = skillIds ?? new List<int>()
            };

            // Truyền dữ liệu qua ViewBag để tương thích với view hiện tại
            ViewBag.Query = query;
            ViewBag.SkillIds = skillIds;
            ViewBag.ProvinceCode = provinceCode;
            ViewBag.Provinces = provinces;
            ViewBag.StudentSkills = studentSkillsDict;

            return View(viewModel);
        }

        public async Task<IActionResult> SearchBusinesses(string query, string provinceCode)
        {
            Console.WriteLine($"[DEBUG] SearchBusinesses called with provinceCode: {provinceCode}");
            
            // Lấy danh sách doanh nghiệp
            var allBusinesses = await _userManager.GetUsersInRoleAsync("Business");
            
            // Lấy danh sách ID của tất cả doanh nghiệp
            var businessIds = allBusinesses.Select(b => b.Id).ToList();
            
            // Lấy thông tin chi tiết của doanh nghiệp bao gồm địa chỉ
            var businessesWithAddress = await _context.Users
                .Include(u => u.Address)
                .Where(u => businessIds.Contains(u.Id) && u.IsActive)
                .ToListAsync();
                
            var businesses = businessesWithAddress.AsQueryable();
            
            // Debug: Hiển thị thông tin địa chỉ của tất cả doanh nghiệp
            foreach (var business in businesses)
            {
                Console.WriteLine($"[DEBUG] Business: {business.FullName}, Address: {business.Address?.ProvinceName ?? "NULL"}, ProvinceCode: {business.Address?.ProvinceCode ?? "NULL"}");
            }

            // Tìm kiếm theo tên công ty hoặc lĩnh vực
            if (!string.IsNullOrEmpty(query))
            {
                string searchQueryLower = query.ToLower();
                businesses = businesses.Where(b => 
                    (b.CompanyName != null && b.CompanyName.ToLower().Contains(searchQueryLower)) || 
                    (b.Industry != null && b.Industry.ToLower().Contains(searchQueryLower)))
                    .AsQueryable();
            }

            // Lọc theo tỉnh/thành phố
            if (!string.IsNullOrEmpty(provinceCode))
            {
                Console.WriteLine($"[DEBUG] Filtering by provinceCode: {provinceCode}");
                
                businesses = businesses.Where(b => 
                    b.Address != null && 
                    b.Address.ProvinceCode == provinceCode)
                    .AsQueryable();
                
                // Debug: Hiển thị số lượng kết quả sau khi lọc
                Console.WriteLine($"[DEBUG] Filtered businesses count: {businesses.Count()}");
                foreach (var business in businesses)
                {
                    Console.WriteLine($"[DEBUG] Filtered Business: {business.FullName}, ProvinceCode: {business.Address?.ProvinceCode ?? "NULL"}");
                }
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

            // Lấy danh sách tỉnh/thành phố từ API
            var apiProvinces = await _locationApiService.GetProvincesAsync();
            ViewBag.Provinces = apiProvinces.Select(p => new
            {
                ID = p.Id,
                Name = p.Name
            }).OrderBy(p => p.Name).ToList();

            ViewBag.Query = query;
            ViewBag.ProvinceCode = provinceCode;

            return View(businesses.ToList());
        }
    }
} 
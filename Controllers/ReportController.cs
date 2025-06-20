using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Interfaces;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;
using StudentFreelance.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.Extensions.Logging;

namespace StudentFreelance.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportService reportService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Report/List
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")] // Chỉ Admin và Moderator mới có thể xem danh sách báo cáo
        public async Task<IActionResult> ListReport(ReportListViewModel model)
        {
            try
            {
                // Nếu model là null hoặc chưa được khởi tạo, tạo một instance mới
                if (model == null)
                {
                    model = new ReportListViewModel();
                }

                // Đảm bảo trang hiện tại và kích thước trang hợp lệ
                if (model.CurrentPage < 1) model.CurrentPage = 1;
                if (model.PageSize < 1) model.PageSize = 10;

                // Lấy danh sách các báo cáo với bộ lọc
                var query = _context.Reports
                    .Include(r => r.Reporter)
                    .Include(r => r.ReportedUser)
                    .Include(r => r.Project)
                    .Include(r => r.Type)
                    .Include(r => r.Status)
                    .Where(r => r.IsActive);
                
                // Mặc định hiển thị các báo cáo đang xử lý (StatusID = 1) nếu không có tìm kiếm và chưa có trạng thái được chọn
                if (string.IsNullOrEmpty(model.SearchQuery) && !model.SelectedTypeId.HasValue && !model.SelectedStatusId.HasValue)
                {
                    // Tìm ID của trạng thái "Đang xử lý"
                    var processingStatusId = await _context.ReportStatuses
                        .Where(s => s.StatusName == "Đang xử lý" && s.IsActive)
                        .Select(s => s.StatusID)
                        .FirstOrDefaultAsync();
                    
                    if (processingStatusId > 0)
                    {
                        // Mặc định chọn trạng thái "Đang xử lý" khi không có trạng thái nào được chọn
                        model.SelectedStatusId = processingStatusId;
                        query = query.Where(r => r.StatusID == processingStatusId);
                    }
                }
                // Nếu có tìm kiếm hoặc lọc, áp dụng các bộ lọc thông thường
                else
                {
                    // Áp dụng bộ lọc theo trạng thái
                    if (model.SelectedStatusId.HasValue)
                    {
                        query = query.Where(r => r.StatusID == model.SelectedStatusId.Value);
                    }

                    // Áp dụng bộ lọc theo loại báo cáo
                    if (model.SelectedTypeId.HasValue)
                    {
                        query = query.Where(r => r.TypeID == model.SelectedTypeId.Value);
                    }

                    // Áp dụng tìm kiếm theo từ khóa
                    if (!string.IsNullOrEmpty(model.SearchQuery))
                    {
                        string searchQuery = model.SearchQuery.ToLower();
                        query = query.Where(r => 
                            r.Description.ToLower().Contains(searchQuery) ||
                            r.Reporter.FullName.ToLower().Contains(searchQuery) ||
                            r.ReportedUser.FullName.ToLower().Contains(searchQuery) ||
                            (r.Project != null && r.Project.Title.ToLower().Contains(searchQuery))
                        );
                    }
                }

                // Đếm tổng số báo cáo thỏa mãn điều kiện
                model.TotalCount = await query.CountAsync();

                // Sắp xếp theo ngày báo cáo giảm dần (mới nhất lên đầu)
                query = query.OrderByDescending(r => r.ReportDate);

                // Phân trang
                var reports = await query
                    .Skip((model.CurrentPage - 1) * model.PageSize)
                    .Take(model.PageSize)
                    .ToListAsync();

                // Chuyển đổi sang ViewModel
                model.Reports = reports.Select(r => new ReportDetailViewModel
                {
                    ReportID = r.ReportID,
                    ReporterName = r.Reporter?.FullName ?? r.Reporter?.UserName ?? "Không xác định",
                    ReportedUserName = r.ReportedUser?.FullName ?? r.ReportedUser?.UserName ?? "Không xác định",
                    ProjectTitle = r.Project?.Title ?? "Không có dự án",
                    ReportDate = r.ReportDate,
                    TypeName = r.Type?.TypeName ?? "Không xác định",
                    Description = r.Description ?? "",
                    StatusName = r.Status?.StatusName ?? "Không xác định",
                    ResolvedAt = r.ResolvedAt,
                    AdminResponse = r.AdminResponse ?? ""
                }).ToList();

                // Lấy danh sách trạng thái cho dropdown lọc
                model.StatusFilter = await _context.ReportStatuses
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.StatusName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StatusID.ToString(),
                        Text = s.StatusName,
                        Selected = model.SelectedStatusId.HasValue && s.StatusID == model.SelectedStatusId.Value
                    }).ToListAsync();

                // Lấy danh sách loại báo cáo cho dropdown lọc
                model.TypeFilter = await _context.ReportTypes
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.TypeName)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TypeID.ToString(),
                        Text = t.TypeName,
                        Selected = model.SelectedTypeId.HasValue && t.TypeID == model.SelectedTypeId.Value
                    }).ToListAsync();

                // Thêm lựa chọn "Tất cả" vào đầu danh sách loại báo cáo
                model.TypeFilter.Insert(0, new SelectListItem { Value = "", Text = "Tất cả loại báo cáo" });

                return View("ListReport", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách báo cáo");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách báo cáo. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Report/Create
        public async Task<IActionResult> Create()
        {
            // Tạo một model mới
            var model = new ReportViewModel
            {
                ReportTypes = GetReportTypes(),
                Projects = GetUserProjects(),
                UsersList = await GetUsersListAsync()
            };
            
            // Lấy thông tin người dùng đăng nhập hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                model.ReporterID = currentUser.Id;
                model.ReporterName = currentUser.FullName ?? currentUser.UserName;
            }
            
            return View("CreateReport", model);
        }

        // GET: Report/Details/{id}
        [HttpGet]
        [AllowAnonymous] // Cho phép xem chi tiết báo cáo mà không cần đăng nhập
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var report = await _reportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy báo cáo.";
                    return RedirectToAction("ListReport");
                }

                var model = new ReportDetailViewModel
                {
                    ReportID = report.ReportID,
                    ReporterName = report.Reporter?.FullName ?? report.Reporter?.UserName ?? "Không xác định",
                    ReportedUserName = report.ReportedUser?.FullName ?? report.ReportedUser?.UserName ?? "Không xác định",
                    ProjectTitle = report.Project?.Title ?? "Không có dự án",
                    ReportDate = report.ReportDate,
                    TypeName = report.Type?.TypeName ?? "Không xác định",
                    Description = report.Description ?? "",
                    StatusName = report.Status?.StatusName ?? "Không xác định",
                    ResolvedAt = report.ResolvedAt,
                    AdminResponse = report.AdminResponse ?? ""
                };

                // Lấy danh sách trạng thái báo cáo cho dropdown nếu người dùng là Admin hoặc Moderator
                if (User.IsInRole("Admin") || User.IsInRole("Moderator"))
                {
                    var reportStatuses = await _context.ReportStatuses
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.StatusID)
                        .Select(s => new SelectListItem
                        {
                            Value = s.StatusID.ToString(),
                            Text = s.StatusName,
                            Selected = s.StatusName == model.StatusName
                        })
                        .ToListAsync();

                    ViewBag.ReportStatuses = reportStatuses;
                }

                return View("DetailReport", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xem chi tiết báo cáo ID={id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải chi tiết báo cáo.";
                return RedirectToAction("ListReport");
            }
        }
        
        // POST: Report/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportViewModel model)
        {
            try
            {
                _logger.LogInformation("Bắt đầu xử lý tạo báo cáo mới");
                
                // Kiểm tra ModelState
                if (ModelState.IsValid)
                {
                    try
                    {
                        // Kiểm tra xem ReportedUserID có tồn tại không
                        var reportedUser = await _userManager.FindByIdAsync(model.ReportedUserID.ToString());
                        if (reportedUser == null)
                        {
                            ModelState.AddModelError("ReportedUserID", "Không tìm thấy người dùng cần báo cáo");
                            model.ReportTypes = GetReportTypes();
                            model.Projects = GetUserProjects();
                            model.UsersList = await GetUsersListAsync();
                            return View("CreateReport", model);
                        }
                        
                        // Lấy trạng thái "Đang xử lý" từ database
                        var pendingStatus = await _context.ReportStatuses
                            .FirstOrDefaultAsync(s => s.StatusName == "Đang xử lý" && s.IsActive);
                        
                        if (pendingStatus == null)
                        {
                            // Nếu không tìm thấy, lấy status đầu tiên có trong database
                            pendingStatus = await _context.ReportStatuses
                                .Where(s => s.IsActive)
                                .FirstOrDefaultAsync();
                                
                            if (pendingStatus == null)
                            {
                                // Tạo một ReportStatus mặc định nếu không có trong cơ sở dữ liệu
                                pendingStatus = new ReportStatus 
                                { 
                                    StatusName = "Đang xử lý", 
                                    IsActive = true 
                                };
                                
                                _context.ReportStatuses.Add(pendingStatus);
                                await _context.SaveChangesAsync();
                            }
                        }
                        
                        // Kiểm tra TypeID có tồn tại không
                        var reportType = await _context.ReportTypes
                            .FirstOrDefaultAsync(t => t.TypeID == model.TypeID && t.IsActive);
                        
                        if (reportType == null)
                        {
                            ModelState.AddModelError("TypeID", "Loại báo cáo không tồn tại");
                            model.ReportTypes = GetReportTypes();
                            model.Projects = GetUserProjects();
                            model.UsersList = await GetUsersListAsync();
                            return View("CreateReport", model);
                        }
                        
                        // Kiểm tra ProjectID nếu được cung cấp
                        if (model.ProjectID.HasValue)
                        {
                            var project = await _context.Projects
                                .FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID.Value && p.IsActive);
                            
                            if (project == null)
                            {
                                ModelState.AddModelError("ProjectID", "Dự án không tồn tại");
                                model.ReportTypes = GetReportTypes();
                                model.Projects = GetUserProjects();
                                model.UsersList = await GetUsersListAsync();
                                return View("CreateReport", model);
                            }
                        }
                        
                        // Lấy ReporterID từ người dùng đăng nhập hiện tại
                        var currentUser = await _userManager.GetUserAsync(User);
                        if (currentUser == null)
                        {
                            ModelState.AddModelError("", "Bạn cần đăng nhập để tạo báo cáo");
                            model.ReportTypes = GetReportTypes();
                            model.Projects = GetUserProjects();
                            model.UsersList = await GetUsersListAsync();
                            return View("CreateReport", model);
                        }
                        
                        int reporterID = currentUser.Id;
                        
                        // Tạo báo cáo mới
                        var report = new Report
                        {
                            ReporterID = reporterID,
                            ReportedUserID = model.ReportedUserID,
                            ProjectID = model.ProjectID,
                            TypeID = model.TypeID,
                            Description = model.Description,
                            StatusID = pendingStatus.StatusID,
                            ReportDate = DateTime.Now,
                            IsActive = true
                        };
                        
                        try
                        {
                            // Thêm báo cáo vào database thông qua service
                            await _reportService.CreateReportAsync(report);
                            
                            TempData["SuccessMessage"] = "Báo cáo đã được gửi thành công!";
                            
                            // Tạo model mới để xóa dữ liệu đã nhập và hiển thị thông báo thành công
                            var newModel = new ReportViewModel
                            {
                                ReportTypes = GetReportTypes(),
                                Projects = GetUserProjects(),
                                UsersList = await GetUsersListAsync(),
                                ReporterID = currentUser.Id,
                                ReporterName = currentUser.FullName ?? currentUser.UserName
                            };
                            
                            // Trả về view với model mới để người dùng có thể tạo báo cáo khác
                            return View("CreateReport", newModel);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Lỗi khi lưu báo cáo vào database: {ex.Message}");
                            ModelState.AddModelError("", $"Lỗi khi lưu báo cáo: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi trong quá trình xử lý tạo báo cáo: {ex.Message}");
                        ModelState.AddModelError("", $"Lỗi khi xử lý báo cáo: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo");
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }
            
            // Nếu ModelState không hợp lệ hoặc có lỗi, làm mới các danh sách dropdown
            model.ReportTypes = GetReportTypes();
            model.Projects = GetUserProjects();
            model.UsersList = await GetUsersListAsync();
            
            return View("CreateReport", model);
        }
        
        // Phương thức helper để lấy danh sách loại báo cáo
        private IEnumerable<SelectListItem> GetReportTypes()
        {
            var types = _context.ReportTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.TypeName)
                .Select(t => new SelectListItem
                {
                    Value = t.TypeID.ToString(),
                    Text = t.TypeName
                }).ToList();
                
            return types;
        }
        
        // Phương thức helper để lấy danh sách dự án
        private IEnumerable<SelectListItem> GetUserProjects()
        {
            // Lấy tất cả dự án để test
            var allProjects = _context.Projects
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectID.ToString(),
                    Text = p.Title
                }).ToList();
                
            return allProjects;
        }
        
        // Helper method để lấy danh sách người dùng
        private async Task<IEnumerable<SelectListItem>> GetUsersListAsync()
        {
            return await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FullName ?? u.UserName
                })
                .Take(50) // Giới hạn 50 người dùng để tránh quá tải
                .ToListAsync();
        }


        
        // POST: Report/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")] // Chỉ Admin và Moderator mới có thể cập nhật trạng thái
        public async Task<IActionResult> UpdateStatus(int reportId, int statusId, string adminResponse)
        {
            try
            {
                if (reportId <= 0 || statusId <= 0)
                {
                    TempData["ErrorMessage"] = "Thông tin báo cáo hoặc trạng thái không hợp lệ.";
                    return RedirectToAction("Details", new { id = reportId });
                }

                // Kiểm tra xem báo cáo có tồn tại không
                var report = await _context.Reports.FindAsync(reportId);
                if (report == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy báo cáo.";
                    return RedirectToAction("ListReport");
                }

                // Kiểm tra xem trạng thái có tồn tại không
                var status = await _context.ReportStatuses.FindAsync(statusId);
                if (status == null)
                {
                    TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                    return RedirectToAction("Details", new { id = reportId });
                }

                // Cập nhật trạng thái báo cáo
                report.StatusID = statusId;
                
                // Nếu là trạng thái "Đã xử lý" hoặc "Đã hủy", cập nhật thời gian xử lý
                if (status.StatusName == "Đã xử lý" || status.StatusName == "Đã hủy")
                {
                    report.ResolvedAt = DateTime.Now;
                }
                
                // Cập nhật phản hồi từ admin nếu có
                if (!string.IsNullOrWhiteSpace(adminResponse))
                {
                    report.AdminResponse = adminResponse;
                }

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật trạng thái báo cáo thành công.";
                return RedirectToAction("Details", new { id = reportId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật trạng thái báo cáo ID={reportId}: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái báo cáo.";
                return RedirectToAction("Details", new { id = reportId });
            }
        }
    }
}

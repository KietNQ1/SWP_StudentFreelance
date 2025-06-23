using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationController(
            IApplicationService applicationService, 
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _applicationService = applicationService;
            _context = context;
            _userManager = userManager;
        }

        // GET: /Application/Apply/{projectId}
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Apply(int projectId)
        {
            // Kiểm tra dự án có tồn tại không
            var project = await _context.Projects
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.IsActive);
                
            if (project == null)
                return NotFound();

            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Kiểm tra xem sinh viên đã ứng tuyển vào dự án này chưa
            var hasApplied = await _applicationService.HasStudentAppliedAsync(projectId, userId);
            if (hasApplied)
            {
                TempData["ErrorMessage"] = "Bạn đã ứng tuyển vào dự án này rồi.";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            // Tạo model cho view
            var model = new CreateApplicationViewModel
            {
                ProjectID = project.ProjectID,
                ProjectTitle = project.Title,
                BusinessName = project.Business?.FullName ?? project.Business?.CompanyName ?? "Không xác định",
                ProjectBudget = project.Budget,
                Salary = project.Budget // Mặc định là budget của dự án, người dùng có thể thay đổi
            };

            return View(model);
        }

        // POST: /Application/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Apply(CreateApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Lấy lại thông tin dự án nếu model không hợp lệ
                var project = await _context.Projects
                    .Include(p => p.Business)
                    .FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID && p.IsActive);
                
                if (project != null)
                {
                    model.ProjectTitle = project.Title;
                    model.BusinessName = project.Business?.FullName ?? project.Business?.CompanyName ?? "Không xác định";
                    model.ProjectBudget = project.Budget;
                }
                
                return View(model);
            }

            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                // Kiểm tra xem đã ứng tuyển chưa
                bool alreadyApplied = await _applicationService.HasStudentAppliedAsync(model.ProjectID, userId);
                if (alreadyApplied)
                {
                    TempData["ErrorMessage"] = "Bạn đã ứng tuyển vào dự án này rồi.";
                    return RedirectToAction("Details", "Project", new { id = model.ProjectID });
                }
                
                // Kiểm tra xem dự án có tồn tại không
                var project = await _context.Projects.FindAsync(model.ProjectID);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Dự án không tồn tại.";
                    return RedirectToAction("Index", "Project");
                }
                
                // Sử dụng service để tạo đơn ứng tuyển
                var application = new StudentApplication
                {
                    ProjectID = model.ProjectID,
                    UserID = userId,
                    CoverLetter = model.CoverLetter,
                    Salary = model.Salary,
                    DateApplied = DateTime.Now,
                    Status = "Pending",
                    IsActive = true
                };

                // Gọi service để tạo đơn ứng tuyển
                var result = await _applicationService.CreateApplicationAsync(application);
                
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Đơn ứng tuyển của bạn đã được gửi thành công!";
                    TempData["DebugInfo"] = $"Đã tạo đơn ứng tuyển với ID: {result.ApplicationID}";
                    return RedirectToAction("MyApplications");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể gửi đơn ứng tuyển. Vui lòng thử lại sau.";
                    return RedirectToAction("Details", "Project", new { id = model.ProjectID });
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết để debug
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi gửi đơn ứng tuyển: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["DebugInfo"] = $"Chi tiết lỗi: {ex.InnerException.Message}";
                }
                
                ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi đơn ứng tuyển. Vui lòng thử lại sau.");
                return View(model);
            }
        }
        
        // GET: /Application/MyApplications
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var applications = await _applicationService.GetApplicationsByStudentIdAsync(userId);
            
            var model = new StudentApplicationListViewModel
            {
                Applications = applications.Select(a => new ApplicationDetailViewModel
                {
                    ApplicationID = a.ApplicationID,
                    ProjectID = a.ProjectID,
                    ProjectTitle = a.Project.Title,
                    Status = a.Status,
                    Salary = a.Salary,
                    DateApplied = a.DateApplied,
                    TimeAgo = GetTimeAgo(a.DateApplied)
                }).ToList()
            };
            
            return View(model);
        }
        
        // GET: /Application/ProjectApplications/{projectId}
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> ProjectApplications(int projectId)
        {
            // Kiểm tra dự án có tồn tại không
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound();
            
            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            
            if (project.BusinessID != userId && !isAdmin)
                return Forbid();
            
            // Kiểm tra trực tiếp trạng thái trong database để gỡ lỗi
            var rawApplications = await _context.StudentApplications
                .Where(a => a.ProjectID == projectId && a.IsActive)
                .ToListAsync();
                
            // Tạo thông báo gỡ lỗi
            string debugInfo = "Trạng thái ứng tuyển trong database: ";
            foreach (var app in rawApplications)
            {
                debugInfo += $"ID: {app.ApplicationID}, Status: {app.Status}; ";
            }
            
            // Lưu thông tin gỡ lỗi vào TempData
            TempData["DebugInfo"] = debugInfo;
            
            // Sử dụng service để lấy danh sách ứng tuyển cho dự án
            var applications = await _applicationService.GetApplicationsByProjectIdAsync(projectId);
            
            var model = new ProjectApplicationListViewModel
            {
                ProjectID = projectId,
                ProjectTitle = project.Title,
                Applications = applications
            };
            
            return View(model);
        }
        
        // GET: /Application/UpdateStatus/{id}
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null)
                return NotFound();
            
            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            
            if (application.Project.BusinessID != userId && !isAdmin)
                return Forbid();
            
            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Đang chờ xử lý", Selected = (application.Status == "Pending") },
                new SelectListItem { Value = "Shortlisted", Text = "Đã lọt vào vòng trong", Selected = (application.Status == "Shortlisted") },
                new SelectListItem { Value = "Accepted", Text = "Đã được chấp nhận", Selected = (application.Status == "Accepted") },
                new SelectListItem { Value = "Rejected", Text = "Đã từ chối", Selected = (application.Status == "Rejected") }
            };
            
            var model = new UpdateApplicationStatusViewModel
            {
                ApplicationID = application.ApplicationID,
                Status = application.Status,
                StatusOptions = statusOptions
            };
            
            // Lưu ProjectID vào ViewBag để sử dụng trong view
            ViewBag.ProjectID = application.ProjectID;
            
            return View(model);
        }
        
        // POST: /Application/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> UpdateStatus(UpdateApplicationStatusViewModel model, int? projectId)
        {
            if (!ModelState.IsValid)
            {
                // Tạo lại danh sách trạng thái nếu ModelState không hợp lệ
                model.StatusOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Đang chờ xử lý", Selected = (model.Status == "Pending") },
                    new SelectListItem { Value = "Shortlisted", Text = "Đã lọt vào vòng trong", Selected = (model.Status == "Shortlisted") },
                    new SelectListItem { Value = "Accepted", Text = "Đã được chấp nhận", Selected = (model.Status == "Accepted") },
                    new SelectListItem { Value = "Rejected", Text = "Đã từ chối", Selected = (model.Status == "Rejected") }
                };
                
                // Nếu có projectId được gửi từ form, sử dụng nó
                if (projectId.HasValue)
                {
                    ViewBag.ProjectID = projectId.Value;
                }
                
                return View(model);
            }
            
            // Lấy ứng dụng từ cơ sở dữ liệu
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.ApplicationID == model.ApplicationID && a.IsActive);
                
            if (application == null)
                return NotFound();
            
            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            
            if (application.Project.BusinessID != userId && !isAdmin)
                return Forbid();
            
            // Lưu lại ProjectID trước khi cập nhật
            int projectIdValue = application.ProjectID;
            
            try {
                // Cập nhật trạng thái
                var result = await _applicationService.UpdateApplicationStatusAsync(model.ApplicationID, model.Status);
                
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn ứng tuyển.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Trạng thái đơn ứng tuyển đã được cập nhật thành công.";
                    // Lưu debug info để xác nhận
                    TempData["DebugInfo"] = $"Đã cập nhật ứng dụng ID: {model.ApplicationID}, Trạng thái mới: {model.Status}";
                }
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = $"Lỗi cập nhật: {ex.Message}";
            }
            
            return RedirectToAction("ProjectApplications", new { projectId = projectIdValue });
        }
        
        // Helper method to format time ago
        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            
            if (span.TotalDays > 30)
                return $"{Math.Floor(span.TotalDays / 30)} tháng trước";
            if (span.TotalDays > 1)
                return $"{Math.Floor(span.TotalDays)} ngày trước";
            if (span.TotalHours > 1)
                return $"{Math.Floor(span.TotalHours)} giờ trước";
            if (span.TotalMinutes > 1)
                return $"{Math.Floor(span.TotalMinutes)} phút trước";
            
            return "Vừa xong";
        }
    }
} 
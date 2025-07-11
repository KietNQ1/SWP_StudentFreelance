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
using System.Text.Json;

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
                .Include(p => p.Status)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.IsActive);
                
            if (project == null)
                return NotFound();
                
            // Kiểm tra xem dự án có đang mở không
            if (project.StatusID != 1) // Assuming 1 is the ID for "Open" status
            {
                TempData["ErrorMessage"] = "Dự án này không còn nhận đơn ứng tuyển.";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

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
            // Debug: Kiểm tra ModelState trước khi xóa các lỗi
            var initialErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
            
            TempData["DebugInitialErrors"] = JsonSerializer.Serialize(initialErrors);
            
            // Xóa các lỗi không liên quan đến việc tạo đơn ứng tuyển
            ModelState.Remove("ProjectTitle");
            ModelState.Remove("BusinessName");
            
            // Xóa các lỗi validation cho các trường mới thêm vào model
            ModelState.Remove("BusinessConfirmedCompletion");
            ModelState.Remove("StudentConfirmedCompletion");
            ModelState.Remove("Notes");
            ModelState.Remove("ResumeLink");
            ModelState.Remove("ResumeAttachment");
            ModelState.Remove("PortfolioLink");
            ModelState.Remove("InterviewDate");
            ModelState.Remove("IsActive");
            
            // Debug: Kiểm tra ModelState sau khi xóa các lỗi
            var remainingErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
            
            TempData["DebugRemainingErrors"] = JsonSerializer.Serialize(remainingErrors);
            
            // Kiểm tra ModelState sau khi đã loại bỏ các trường không cần thiết
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
                var project = await _context.Projects
                    .Include(p => p.Status)
                    .FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID);
                    
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Dự án không tồn tại.";
                    return RedirectToAction("Index", "Project");
                }
                
                // Kiểm tra xem dự án có đang mở không
                if (project.StatusID != 1) // Assuming 1 is the ID for "Open" status
                {
                    TempData["ErrorMessage"] = "Dự án này không còn nhận đơn ứng tuyển.";
                    return RedirectToAction("Details", "Project", new { id = model.ProjectID });
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
                    PortfolioLink = model.PortfolioLink,
                    ResumeAttachment = model.ResumeAttachment,
                    LastStatusUpdate = DateTime.Now,
                    BusinessConfirmedCompletion = false,
                    StudentConfirmedCompletion = false
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
                    TimeAgo = GetTimeAgo(a.DateApplied),
                    LastStatusUpdate = a.LastStatusUpdate
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
                .Where(a => a.ProjectID == projectId)
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
                new SelectListItem { Value = "Accepted", Text = "Đã được chấp nhận", Selected = (application.Status == "Accepted") },
                new SelectListItem { Value = "Rejected", Text = "Đã từ chối", Selected = (application.Status == "Rejected") }
            };
            
            var model = new UpdateApplicationStatusViewModel
            {
                ApplicationID = application.ApplicationID,
                Status = application.Status,
                StatusOptions = statusOptions,
                BusinessNotes = application.BusinessNotes
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
            // Xóa lỗi cho trường StatusOptions vì đây chỉ là trường hiển thị
            ModelState.Remove("StatusOptions");
            
            if (!ModelState.IsValid)
            {
                // Tạo lại danh sách trạng thái nếu ModelState không hợp lệ
                model.StatusOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Đang chờ xử lý", Selected = (model.Status == "Pending") },
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
                .FirstOrDefaultAsync(a => a.ApplicationID == model.ApplicationID);
                
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
                    // Cập nhật thêm các thông tin khác
                    if (!string.IsNullOrEmpty(model.BusinessNotes))
                    {
                        await _applicationService.UpdateBusinessFeedbackAsync(
                            model.ApplicationID, 
                            model.BusinessNotes);
                    }
                    
                    TempData["SuccessMessage"] = "Thông tin đơn ứng tuyển đã được cập nhật thành công.";
                }
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = $"Lỗi cập nhật: {ex.Message}";
            }
            
            return RedirectToAction("ProjectApplications", new { projectId = projectIdValue });
        }
        
        // GET: /Application/ScheduleInterview/{id}
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> ScheduleInterview(int id)
        {
            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null)
                return NotFound();
            
            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            
            if (application.Project.BusinessID != userId && !isAdmin)
                return Forbid();
            
            var model = new ScheduleInterviewViewModel
            {
                ApplicationID = application.ApplicationID,
                StudentName = application.User.FullName,
                ProjectTitle = application.Project.Title,
                InterviewDateTime = DateTime.Now.AddDays(1),
                Notes = ""
            };
            
            // Lưu ProjectID vào ViewBag để sử dụng trong view
            ViewBag.ProjectID = application.ProjectID;
            
            return View(model);
        }
        
        // POST: /Application/ScheduleInterview
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> ScheduleInterview(ScheduleInterviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try {
                var result = await _applicationService.ScheduleInterviewAsync(model.ApplicationID, model.InterviewDateTime);
                
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Không thể lên lịch phỏng vấn.";
                    return View(model);
                }
                
                TempData["SuccessMessage"] = "Đã lên lịch phỏng vấn thành công.";
                return RedirectToAction("ProjectApplications", new { projectId = result.ProjectID });
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = $"Lỗi khi lên lịch phỏng vấn: {ex.Message}";
                return View(model);
            }
        }
        
        // GET: /Application/ViewApplication/{id}
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> ViewApplication(int id)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _applicationService.GetApplicationDetailAsync(id);
            if (application == null)
                return NotFound();
                
            // Kiểm tra xem người dùng hiện tại có phải là chủ dự án không
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (application.Project.BusinessID != userId)
                return Forbid();
                
            return View(application);
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
using System.IO;
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
        private readonly INotificationService _notificationService;

        public ApplicationController(
            IApplicationService applicationService, 
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _applicationService = applicationService;
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: /Application/Apply/{projectId}
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Apply(int projectId)
        {
            // Kiểm tra xem dự án có tồn tại không
            var project = await _context.Projects
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.IsActive);
                
            if (project == null)
            {
                TempData["ErrorMessage"] = "Dự án không tồn tại.";
                return RedirectToAction("Index", "Project");
            }
            
            // Kiểm tra xem dự án có đang mở không
            if (project.StatusID != 1) // Assuming 1 is the ID for "Open" status
            {
                TempData["ErrorMessage"] = "Dự án này không còn nhận đơn ứng tuyển.";
                return RedirectToAction("Details", "Project", new { id = project.ProjectID });
            }
            
            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Kiểm tra xem đã ứng tuyển chưa
            bool alreadyApplied = await _applicationService.HasStudentAppliedAsync(projectId, userId);
            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "Bạn đã ứng tuyển vào dự án này rồi.";
                return RedirectToAction("Details", "Project", new { id = project.ProjectID });
            }
            
            // Tạo model cho view
            var model = new CreateApplicationViewModel
            {
                ProjectID = project.ProjectID,
                ProjectTitle = project.Title,
                BusinessName = project.Business?.FullName ?? project.Business?.CompanyName ?? "Không xác định",
                ProjectBudget = project.Budget
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
            
            // Debug sau khi remove
            var remainingErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
            
            TempData["DebugRemainingErrors"] = JsonSerializer.Serialize(remainingErrors);
            
            // Thêm log để kiểm tra trạng thái resumeFile
            TempData["DebugResumeFile"] = model.ResumeAttachment == null ? "ResumeAttachment is null" : $"ResumeAttachment: {model.ResumeAttachment.FileName}, Length: {model.ResumeAttachment.Length}";

            // Kiểm tra trạng thái ModelState
            if (!ModelState.IsValid)
            {
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
                
                // Xử lý tải lên CV nếu có
                string resumeAttachmentPath = null; // Khởi tạo là null
                if (model.ResumeAttachment != null && model.ResumeAttachment.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var fileExtension = Path.GetExtension(model.ResumeAttachment.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ResumeAttachment", "Chỉ chấp nhận file PDF, DOC hoặc DOCX.");
                        
                        // Lấy lại thông tin dự án nếu model không hợp lệ
                        var projectInfo = await _context.Projects
                            .Include(p => p.Business)
                            .FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID && p.IsActive);
                        
                        if (projectInfo != null)
                        {
                            model.ProjectTitle = projectInfo.Title;
                            model.BusinessName = projectInfo.Business?.FullName ?? projectInfo.Business?.CompanyName ?? "Không xác định";
                            model.ProjectBudget = projectInfo.Budget;
                        }
                        
                        return View(model);
                    }
                    
                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ResumeAttachment.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "submissions");
                    
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    
                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ResumeAttachment.CopyToAsync(stream);
                    }
                    
                    // Cập nhật đường dẫn CV
                    resumeAttachmentPath = "/uploads/submissions/" + fileName;
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
                    ResumeAttachment = resumeAttachmentPath, // Có thể null nếu không có file CV
                    LastStatusUpdate = DateTime.Now,
                    BusinessConfirmedCompletion = false,
                    StudentConfirmedCompletion = false
                };

                // Debug: Kiểm tra thông tin đơn ứng tuyển trước khi tạo
                TempData["DebugApplication"] = $"ProjectID: {application.ProjectID}, UserID: {userId}, ResumeAttachment: {(resumeAttachmentPath == null ? "null" : resumeAttachmentPath)}";

                // Gọi service để tạo đơn ứng tuyển
                var result = await _applicationService.CreateApplicationAsync(application);
                
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Đơn ứng tuyển của bạn đã được gửi thành công!";
                    TempData["DebugInfo"] = $"Đã tạo đơn ứng tuyển với ID: {result.ApplicationID}";
                    
                    // Gửi thông báo cho doanh nghiệp
                    var projectWithBusiness = await _context.Projects.Include(p => p.Business).FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID);
                    if (projectWithBusiness?.Business != null)
                    {
                        await _notificationService.SendNotificationToUserAsync(
                            projectWithBusiness.Business.Id,
                            "Có ứng viên mới cho dự án",
                            $"Sinh viên {User.Identity.Name} vừa ứng tuyển vào dự án '{projectWithBusiness.Title}'.",
                            1, // TypeID hệ thống
                            projectWithBusiness.ProjectID,
                            userId,
                            true // chỉ notification, không gửi email
                        );
                    }

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
                // Nếu đang chấp nhận ứng viên, kiểm tra ngân sách dự án
                if (model.Status == "Accepted" && application.Status != "Accepted")
                {
                    // Inject ProjectService để sử dụng
                    var projectService = HttpContext.RequestServices.GetService<IProjectService>();
                    
                    // Kiểm tra ngân sách
                    var budgetCheck = await projectService.CheckProjectBudgetForAcceptedStudentsAsync(
                        projectIdValue, application.ApplicationID);
                    
                    if (!budgetCheck.IsEnough)
                    {
                        // Nếu ngân sách không đủ, lưu thông tin vào TempData để hiển thị modal
                        TempData["BudgetNotEnough"] = "true";
                        TempData["MissingAmount"] = budgetCheck.MissingAmount.ToString();
                        TempData["ApplicationID"] = application.ApplicationID.ToString();
                        TempData["ProjectID"] = projectIdValue.ToString();
                        TempData["BusinessNotes"] = model.BusinessNotes;
                        
                        // Redirect để hiển thị modal xác nhận
                        return RedirectToAction("ConfirmAcceptStudent", new { 
                            applicationId = application.ApplicationID,
                            missingAmount = budgetCheck.MissingAmount,
                            businessNotes = model.BusinessNotes
                        });
                    }
                }
                
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

                    // Gửi thông báo cho sinh viên
                    var student = await _context.Users.FindAsync(application.UserID);
                    if (student != null)
                    {
                        string title = "Cập nhật trạng thái đơn ứng tuyển";
                        string content = $"Trạng thái đơn ứng tuyển của bạn cho dự án '{application.Project.Title}' đã được cập nhật thành '{model.Status}'.";
                        await _notificationService.SendNotificationToUserAsync(
                            student.Id,
                            title,
                            content,
                            1,
                            application.ProjectID,
                            application.Project.BusinessID,
                            true // chỉ notification, không gửi email
                        );
                    }
                }
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = $"Lỗi cập nhật: {ex.Message}";
            }
            
            return RedirectToAction("ProjectApplications", new { projectId = projectIdValue });
        }
        
        // GET: /Application/ConfirmAcceptStudent
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> ConfirmAcceptStudent(int applicationId, decimal missingAmount, string businessNotes)
        {
            var application = await _applicationService.GetApplicationByIdAsync(applicationId);
            if (application == null)
                return NotFound();
            
            // Kiểm tra người dùng có phải là chủ dự án
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (application.Project.BusinessID != userId)
                return Forbid();
            
            // Lấy thông tin doanh nghiệp để kiểm tra số dư ví
            var business = await _context.Users.FindAsync(userId);
            
            var model = new ConfirmAcceptStudentViewModel
            {
                ApplicationID = applicationId,
                ProjectID = application.ProjectID,
                StudentName = application.User.FullName,
                ProjectTitle = application.Project.Title,
                StudentSalary = application.Salary,
                MissingAmount = missingAmount,
                BusinessNotes = businessNotes,
                WalletBalance = business.WalletBalance
            };
            
            return View(model);
        }
        
        // POST: /Application/ConfirmAcceptStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> ConfirmAcceptStudent(ConfirmAcceptStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Form validation failed. Please check the form and try again.";
                return View(model);
            }
            
            var application = await _applicationService.GetApplicationByIdAsync(model.ApplicationID);
            if (application == null)
                return NotFound();
            
            // Kiểm tra người dùng có phải là chủ dự án
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (application.Project.BusinessID != userId)
                return Forbid();
            
            try
            {
                if (model.ConfirmAddFunds)
                {
                    // Inject ProjectService để sử dụng
                    var projectService = HttpContext.RequestServices.GetService<IProjectService>();
                    
                    // Thêm tiền vào dự án từ ví
                    var addFundsResult = await projectService.AddFundsToProjectFromWalletAsync(
                        model.ProjectID, userId, model.MissingAmount);
                    
                    if (!addFundsResult)
                    {
                        TempData["ErrorMessage"] = "Không thể bổ sung ngân sách cho dự án. Vui lòng kiểm tra số dư ví của bạn.";
                        return View(model);
                    }
                    
                    TempData["SuccessMessage"] = $"Đã bổ sung {model.MissingAmount:N0} VNĐ vào ngân sách dự án.";
                }
                else if (!model.ConfirmAccept)
                {
                    // Nếu không xác nhận, quay lại trang chi tiết đơn ứng tuyển
                    return RedirectToAction("ViewApplication", new { id = model.ApplicationID });
                }
                
                // Cập nhật trạng thái đơn ứng tuyển
                var result = await _applicationService.UpdateApplicationStatusAsync(model.ApplicationID, "Accepted");
                
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn ứng tuyển.";
                }
                else
                {
                    // Cập nhật ghi chú nếu có
                    if (!string.IsNullOrEmpty(model.BusinessNotes))
                    {
                        await _applicationService.UpdateBusinessFeedbackAsync(
                            model.ApplicationID, 
                            model.BusinessNotes);
                    }
                    
                    TempData["SuccessMessage"] = "Đã chấp nhận ứng viên thành công.";

                    // Gửi thông báo cho sinh viên
                    var student = await _context.Users.FindAsync(application.UserID);
                    if (student != null)
                    {
                        string title = "Đơn ứng tuyển của bạn đã được chấp nhận";
                        string content = $"Chúc mừng! Đơn ứng tuyển của bạn cho dự án '{application.Project.Title}' đã được chấp nhận.";
                        await _notificationService.SendNotificationToUserAsync(
                            student.Id,
                            title,
                            content,
                            1,
                            application.ProjectID,
                            application.Project.BusinessID,
                            true // chỉ notification, không gửi email
                        );
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return View(model);
            }
            
            return RedirectToAction("ProjectApplications", new { projectId = model.ProjectID });
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
        [Authorize(Roles = "Business,Student")]
        public async Task<IActionResult> ViewApplication(int id)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _applicationService.GetApplicationDetailAsync(id);
            if (application == null)
                return NotFound();
                
            // Kiểm tra quyền truy cập
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Cho phép sinh viên xem đơn ứng tuyển của chính họ
            if (User.IsInRole("Student") && application.UserID != userId)
                return Forbid();
            
            // Cho phép doanh nghiệp xem đơn ứng tuyển cho dự án của họ
            if (User.IsInRole("Business") && application.Project.BusinessID != userId)
                return Forbid();
                
            return View(application);
        }
        
        // GET: /Application/UpdateStudentApplication/{id}
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentApplication(int id)
        {
            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Lấy thông tin đơn ứng tuyển
            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null)
                return NotFound();
            
            // Kiểm tra xem đơn ứng tuyển có thuộc về sinh viên này không
            if (application.UserID != userId)
                return Forbid();
            
            // Kiểm tra trạng thái đơn ứng tuyển, chỉ cho phép cập nhật nếu đang ở trạng thái Pending
            if (application.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể cập nhật đơn ứng tuyển khi đơn đang ở trạng thái chờ xử lý.";
                return RedirectToAction("ViewApplication", new { id = application.ApplicationID });
            }
            
            // Lấy thông tin dự án
            var project = await _context.Projects
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == application.ProjectID);
            
            if (project == null)
                return NotFound();
            
            // Tạo model cho view
            var model = new UpdateStudentApplicationViewModel
            {
                ApplicationID = application.ApplicationID,
                ProjectID = application.ProjectID,
                ProjectTitle = project.Title,
                BusinessName = project.Business?.FullName ?? project.Business?.CompanyName ?? "Không xác định",
                ProjectBudget = project.Budget,
                CoverLetter = application.CoverLetter,
                Salary = (int)application.Salary, // Chuyển đổi sang int để loại bỏ phần thập phân
                PortfolioLink = application.PortfolioLink,
                CurrentResumeAttachment = application.ResumeAttachment
            };
            
            return View(model);
        }

        // POST: /Application/UpdateStudentApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentApplication(UpdateStudentApplicationViewModel model)
        {
            // Lấy thông tin người dùng hiện tại
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Lấy thông tin đơn ứng tuyển
            var application = await _applicationService.GetApplicationByIdAsync(model.ApplicationID);
            if (application == null)
                return NotFound();
            
            // Kiểm tra xem đơn ứng tuyển có thuộc về sinh viên này không
            if (application.UserID != userId)
                return Forbid();
            
            // Kiểm tra trạng thái đơn ứng tuyển, chỉ cho phép cập nhật nếu đang ở trạng thái Pending
            if (application.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể cập nhật đơn ứng tuyển khi đơn đang ở trạng thái chờ xử lý.";
                return RedirectToAction("ViewApplication", new { id = model.ApplicationID });
            }
            
            // Xóa các lỗi không liên quan đến việc cập nhật đơn ứng tuyển
            ModelState.Remove("ProjectTitle");
            ModelState.Remove("BusinessName");
            ModelState.Remove("ProjectBudget");
            ModelState.Remove("ResumeAttachment"); // Xóa lỗi liên quan đến ResumeAttachment
            ModelState.Remove("CurrentResumeAttachment"); // Xóa lỗi liên quan đến CurrentResumeAttachment
            
            // Debug: Kiểm tra ModelState
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
            
            TempData["DebugErrors"] = JsonSerializer.Serialize(errors);
            
            // Debug: Kiểm tra ResumeAttachment
            TempData["DebugResumeAttachment"] = model.ResumeAttachment == null ? "ResumeAttachment is null" : $"ResumeAttachment: {model.ResumeAttachment.FileName}, Length: {model.ResumeAttachment.Length}";
            
            if (!ModelState.IsValid)
            {
                // Lấy thông tin dự án
                var project = await _context.Projects
                    .Include(p => p.Business)
                    .FirstOrDefaultAsync(p => p.ProjectID == application.ProjectID);
                
                if (project != null)
                {
                    model.ProjectTitle = project.Title;
                    model.BusinessName = project.Business?.FullName ?? project.Business?.CompanyName ?? "Không xác định";
                    model.ProjectBudget = project.Budget;
                }
                
                return View(model);
            }
            
            try
            {
                // Xử lý tải lên CV mới nếu có
                string newResumeAttachmentPath = null;
                if (model.ResumeAttachment != null && model.ResumeAttachment.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var fileExtension = Path.GetExtension(model.ResumeAttachment.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ResumeAttachment", "Chỉ chấp nhận file PDF, DOC hoặc DOCX.");
                         
                        // Lấy lại thông tin dự án
                        var projectDetails = await _context.Projects
                            .Include(p => p.Business)
                            .FirstOrDefaultAsync(p => p.ProjectID == application.ProjectID);
                         
                        if (projectDetails != null)
                        {
                            model.ProjectTitle = projectDetails.Title;
                            model.BusinessName = projectDetails.Business?.FullName ?? projectDetails.Business?.CompanyName ?? "Không xác định";
                            model.ProjectBudget = projectDetails.Budget;
                        }
                        
                        return View(model);
                    }
                    
                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ResumeAttachment.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "submissions");
                    
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    
                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ResumeAttachment.CopyToAsync(stream);
                    }
                    
                    // Cập nhật đường dẫn CV mới
                    newResumeAttachmentPath = "/uploads/submissions/" + fileName;
                }
                
                // Cập nhật thông tin đơn ứng tuyển
                application.CoverLetter = model.CoverLetter;
                application.Salary = model.Salary;
                application.PortfolioLink = model.PortfolioLink;
                
                // Chỉ cập nhật ResumeAttachment nếu có file mới được tải lên
                if (newResumeAttachmentPath != null)
                {
                    application.ResumeAttachment = newResumeAttachmentPath;
                }
                
                // Debug: Kiểm tra thông tin đơn ứng tuyển trước khi cập nhật
                TempData["DebugUpdateApplication"] = $"ApplicationID: {application.ApplicationID}, ResumeAttachment: {application.ResumeAttachment ?? "null"}";
                
                // Lưu thay đổi vào database
                _context.StudentApplications.Update(application);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đơn ứng tuyển của bạn đã được cập nhật thành công!";
                
                // Gửi thông báo cho doanh nghiệp
                var project = await _context.Projects
                    .Include(p => p.Business)
                    .FirstOrDefaultAsync(p => p.ProjectID == application.ProjectID);
                    
                if (project?.Business != null)
                {
                    await _notificationService.SendNotificationToUserAsync(
                        project.Business.Id,
                        "Đơn ứng tuyển đã được cập nhật",
                        $"Sinh viên {User.Identity.Name} vừa cập nhật đơn ứng tuyển vào dự án '{project.Title}'.",
                        1, // TypeID hệ thống
                        project.ProjectID,
                        userId,
                        true // chỉ notification, không gửi email
                    );
                }
                
                return RedirectToAction("ViewApplication", new { id = model.ApplicationID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi cập nhật đơn ứng tuyển: {ex.Message}";
                return View(model);
            }
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
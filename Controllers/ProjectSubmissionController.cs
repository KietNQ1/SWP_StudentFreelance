using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class ProjectSubmissionController : Controller
    {
        private readonly IProjectSubmissionService _submissionService;
        private readonly IApplicationService _applicationService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectSubmissionController(
            IProjectSubmissionService submissionService,
            IApplicationService applicationService,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _submissionService = submissionService;
            _applicationService = applicationService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ProjectSubmissionController
        public ActionResult Index()
        {
            return View();
        }

        // GET: /ProjectSubmission/Create/{applicationId}
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(int applicationId)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return NotFound();

            // Kiểm tra xem người dùng hiện tại có phải là người sở hữu đơn ứng tuyển này không
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (application.UserID != currentUserId)
                return Forbid();

            // Kiểm tra xem trạng thái đơn ứng tuyển có cho phép nộp kết quả không
            if (application.Status != "Accepted" && application.Status != "InProgress")
            {
                TempData["ErrorMessage"] = "Bạn không thể nộp kết quả dự án ở trạng thái hiện tại.";
                return RedirectToAction("MyApplications", "Application");
            }

            var model = new ProjectSubmissionViewModel
            {
                ApplicationID = applicationId,
                ProjectTitle = application.Project.Title,
                StudentName = application.User.FullName,
                BusinessName = application.Project.Business?.FullName ?? application.Project.Business?.CompanyName ?? "Không xác định"
            };

            ViewBag.ProjectId = application.ProjectID;
            return View(model);
        }

        // POST: /ProjectSubmission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(ProjectSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Lấy lại thông tin application nếu model không hợp lệ
                var application = await _context.StudentApplications
                    .Include(a => a.Project)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.ApplicationID == model.ApplicationID);

                if (application != null)
                {
                    model.ProjectTitle = application.Project.Title;
                    model.StudentName = application.User.FullName;
                    model.BusinessName = application.Project.Business?.FullName ?? application.Project.Business?.CompanyName ?? "Không xác định";
                    ViewBag.ProjectId = application.ProjectID;
                }

                return View(model);
            }

            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var app = await _context.StudentApplications
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.ApplicationID == model.ApplicationID);

            if (app == null)
                return NotFound();

            // Kiểm tra xem người dùng hiện tại có phải là người sở hữu đơn ứng tuyển này không
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (app.UserID != currentUserId)
                return Forbid();

            // Tạo submission mới
            var submission = new ProjectSubmission
            {
                ApplicationID = model.ApplicationID,
                Title = model.Title,
                Description = model.Description,
                SubmittedAt = DateTime.Now,
                Status = "Pending",
                IsActive = true
            };

            // Lưu submission
            var createdSubmission = await _submissionService.CreateSubmissionAsync(submission);

            // Xử lý file đính kèm
            if (model.Attachments != null && model.Attachments.Any(f => f != null && f.Length > 0))
            {
                try
                {
                    await SaveAttachments(model.Attachments, createdSubmission.SubmissionID, currentUserId);
                }
                catch (Exception ex)
                {
                    TempData["AttachmentError"] = "Kết quả dự án đã được nộp, nhưng có lỗi khi upload file đính kèm.";
                    Console.WriteLine($"Error uploading attachments: {ex}");
                }
            }

            TempData["SuccessMessage"] = "Kết quả dự án đã được nộp thành công!";
            
            // Chuyển hướng về trang Project Details
            var projectId = app.ProjectID;
            return RedirectToAction("Details", "Project", new { id = projectId });
        }

        // GET: /ProjectSubmission/MySubmissions/{applicationId}
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MySubmissions(int applicationId)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return NotFound();

            // Kiểm tra xem người dùng hiện tại có phải là người sở hữu đơn ứng tuyển này không
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (application.UserID != currentUserId)
                return Forbid();

            // Lấy danh sách submission
            var submissions = await _submissionService.GetSubmissionsByApplicationIdAsync(applicationId);

            var model = new ProjectSubmissionListViewModel
            {
                ApplicationID = applicationId,
                ProjectTitle = application.Project.Title,
                StudentName = application.User.FullName,
                ApplicationStatus = application.Status,
                Submissions = submissions.Select(s => new ProjectSubmissionViewModel
                {
                    SubmissionID = s.SubmissionID,
                    ApplicationID = s.ApplicationID,
                    Title = s.Title,
                    Description = s.Description,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Status,
                    BusinessFeedback = s.BusinessFeedback,
                    FeedbackDate = s.FeedbackDate,
                    ExistingAttachments = s.Attachments.ToList()
                }).ToList()
            };

            ViewBag.ProjectId = application.ProjectID;
            return View(model);
        }

        // GET: /ProjectSubmission/ProjectSubmissions/{applicationId}
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> ProjectSubmissions(int applicationId)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return NotFound();

            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (application.Project.BusinessID != currentUserId && !isAdmin)
                return Forbid();

            // Lấy danh sách submission
            var submissions = await _submissionService.GetSubmissionsByApplicationIdAsync(applicationId);

            var model = new ProjectSubmissionListViewModel
            {
                ApplicationID = applicationId,
                ProjectTitle = application.Project.Title,
                StudentName = application.User.FullName,
                ApplicationStatus = application.Status,
                Submissions = submissions.Select(s => new ProjectSubmissionViewModel
                {
                    SubmissionID = s.SubmissionID,
                    ApplicationID = s.ApplicationID,
                    Title = s.Title,
                    Description = s.Description,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Status,
                    BusinessFeedback = s.BusinessFeedback,
                    FeedbackDate = s.FeedbackDate,
                    ExistingAttachments = s.Attachments.ToList()
                }).ToList()
            };

            ViewBag.ProjectId = application.ProjectID;
            return View(model);
        }

        // GET: /ProjectSubmission/Details/{id}
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id);
            if (submission == null)
                return NotFound();

            // Kiểm tra quyền truy cập
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");
            bool isStudent = User.IsInRole("Student");
            bool isBusiness = User.IsInRole("Business");

            if (isStudent && submission.Application.UserID != currentUserId)
                return Forbid();

            if (isBusiness && submission.Application.Project.BusinessID != currentUserId)
                return Forbid();

            if (!isAdmin && !isStudent && !isBusiness)
                return Forbid();

            var model = new ProjectSubmissionViewModel
            {
                SubmissionID = submission.SubmissionID,
                ApplicationID = submission.ApplicationID,
                Title = submission.Title,
                Description = submission.Description,
                SubmittedAt = submission.SubmittedAt,
                Status = submission.Status,
                BusinessFeedback = submission.BusinessFeedback,
                FeedbackDate = submission.FeedbackDate,
                ExistingAttachments = submission.Attachments.ToList(),
                ProjectTitle = submission.Application.Project.Title,
                StudentName = submission.Application.User.FullName,
                BusinessName = submission.Application.Project.Business?.FullName ?? submission.Application.Project.Business?.CompanyName ?? "Không xác định"
            };

            ViewBag.ProjectId = submission.Application.ProjectID;
            return View(model);
        }

        // GET: /ProjectSubmission/Feedback/{id}
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> Feedback(int id)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id);
            if (submission == null)
                return NotFound();

            // Kiểm tra quyền truy cập
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (submission.Application.Project.BusinessID != currentUserId && !isAdmin)
                return Forbid();

            var model = new ProjectSubmissionFeedbackViewModel
            {
                SubmissionID = submission.SubmissionID,
                Feedback = submission.BusinessFeedback
            };

            ViewBag.ProjectId = submission.Application.ProjectID;
            return View(model);
        }

        // POST: /ProjectSubmission/Feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> Feedback(ProjectSubmissionFeedbackViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var submission = await _submissionService.GetSubmissionByIdAsync(model.SubmissionID);
            if (submission == null)
                return NotFound();

            // Kiểm tra quyền truy cập
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (submission.Application.Project.BusinessID != currentUserId && !isAdmin)
                return Forbid();

            // Xử lý phản hồi
            if (model.IsApproved)
            {
                await _submissionService.ApproveSubmissionAsync(model.SubmissionID, model.Feedback);
                TempData["SuccessMessage"] = "Đã chấp nhận kết quả dự án.";
            }
            else
            {
                await _submissionService.RejectSubmissionAsync(model.SubmissionID, model.Feedback);
                TempData["SuccessMessage"] = "Đã từ chối kết quả dự án và yêu cầu sinh viên nộp lại.";
            }

            // Chuyển hướng về trang Project Details
            var projectId = submission.Application.ProjectID;
            return RedirectToAction("Details", "Project", new { id = projectId });
        }

        // GET: /ProjectSubmission/Finalize/{applicationId}
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> Finalize(int applicationId)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return NotFound();

            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (application.Project.BusinessID != currentUserId && !isAdmin)
                return Forbid();

            // Kiểm tra xem đơn ứng tuyển có ở trạng thái "Completed" không
            if (application.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Dự án chưa được hoàn thành, không thể thanh toán.";
                return RedirectToAction("ProjectSubmissions", new { applicationId });
            }

            // Kiểm tra xem đã thanh toán chưa
            if (application.IsPaid)
            {
                TempData["ErrorMessage"] = "Dự án này đã được thanh toán trước đó.";
                return RedirectToAction("ProjectSubmissions", new { applicationId });
            }

            ViewBag.ProjectId = application.ProjectID;
            return View(application);
        }

        // POST: /ProjectSubmission/Finalize
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Business,Admin,Moderator")]
        public async Task<IActionResult> FinalizeConfirmed(int applicationId)
        {
            // Kiểm tra xem đơn ứng tuyển có tồn tại không
            var application = await _context.StudentApplications
                .Include(a => a.Project)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
                return NotFound();

            // Kiểm tra người dùng có phải là chủ dự án hoặc admin/moderator
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (application.Project.BusinessID != currentUserId && !isAdmin)
                return Forbid();

            // Kiểm tra xem đơn ứng tuyển có ở trạng thái "Completed" không
            if (application.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Dự án chưa được hoàn thành, không thể thanh toán.";
                return RedirectToAction("ProjectSubmissions", new { applicationId });
            }

            // Kiểm tra xem đã thanh toán chưa
            if (application.IsPaid)
            {
                TempData["ErrorMessage"] = "Dự án này đã được thanh toán trước đó.";
                return RedirectToAction("ProjectSubmissions", new { applicationId });
            }

            // Thực hiện thanh toán
            var result = await _submissionService.FinalizeProjectAsync(applicationId);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã thanh toán thành công cho sinh viên.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán. Vui lòng thử lại sau.";
            }

            // Chuyển hướng về trang Project Details
            var projectId = application.ProjectID;
            return RedirectToAction("Details", "Project", new { id = projectId });
        }

        // Helper method to save attachments
        private async Task SaveAttachments(List<IFormFile> files, int submissionId, int uploaderId)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "submissions");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file to disk
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create attachment record
                    var attachment = new ProjectSubmissionAttachment
                    {
                        SubmissionID = submissionId,
                        FileName = Path.GetFileName(file.FileName),
                        FilePath = "/uploads/submissions/" + uniqueFileName,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        UploadedAt = DateTime.Now,
                        UploadedBy = uploaderId,
                        IsActive = true
                    };

                    await _submissionService.SaveSubmissionAttachmentAsync(attachment);
                }
            }
        }
    }
}

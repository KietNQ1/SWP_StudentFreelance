using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.Data;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentFreelance.Controllers
{
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // STUDENT → BUSINESS
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(int projectId, int revieweeId)
        {
            var project = await _context.Projects
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);

            var business = await _context.Users.FindAsync(revieweeId);
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (project == null || business == null)
                return NotFound();

            // Kiểm tra quyền
            var isOwned = await _context.StudentApplications
                .AnyAsync(a => a.ProjectID == projectId && a.UserID == currentUserId && a.Status == "Completed");
            if (!isOwned)
                return Forbid();

            // Tránh đánh giá trùng
            var existing = await _context.Ratings.FirstOrDefaultAsync(r =>
                r.ProjectID == projectId &&
                r.ReviewerID == currentUserId &&
                r.RevieweeID == revieweeId);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá doanh nghiệp này.";
                return RedirectToAction("MyProjects", "Project");
            }

            var model = new CreateRatingViewModel
            {
                ProjectID = projectId,
                ReviewerID = currentUserId,
                RevieweeID = revieweeId,
                ProjectTitle = project.Title,
                RevieweeName = business.FullName ?? business.Email
            };

            return View("Create", model);
        }
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRatingViewModel model)
        {
            // ✅ Xoá thủ công 2 trường không cần validate
            ModelState.Remove(nameof(model.ProjectTitle));
            ModelState.Remove(nameof(model.RevieweeName));
            ModelState.Remove(nameof(model.ApplicationId));

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"[ModelState Error] Field: {key}, Error: {error.ErrorMessage}");
                    }
                }

                return View("Create", model);
            }

            var rating = new Rating
            {
                ProjectID = model.ProjectID,
                ReviewerID = model.ReviewerID,
                RevieweeID = model.RevieweeID,
                Score = model.Score,
                Comment = model.Comment,
                DateRated = DateTime.Now,
                IsActive = true
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi đánh giá thành công.";
            return RedirectToAction("MySubmissions", "ProjectSubmission", new { applicationId = model.ApplicationId });

        }


        // BUSINESS → STUDENT
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> CreateForStudent(int projectId, int revieweeId)
        {
            var project = await _context.Projects
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);

            var student = await _context.Users.FindAsync(revieweeId);
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (project == null || student == null)
                return NotFound();

            if (project.BusinessID != currentUserId)
                return Forbid();

            var existing = await _context.Ratings.FirstOrDefaultAsync(r =>
                r.ProjectID == projectId &&
                r.ReviewerID == currentUserId &&
                r.RevieweeID == revieweeId);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá sinh viên này.";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            var model = new CreateRatingViewModel
            {
                ProjectID = projectId,
                ReviewerID = currentUserId,
                RevieweeID = revieweeId,
                ProjectTitle = project.Title,
                RevieweeName = student.FullName ?? student.Email
            };

            return View("Create", model);
        }

        [HttpPost]
        [Authorize(Roles = "Business")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForStudent(CreateRatingViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Create", model);

            var rating = new Rating
            {
                ProjectID = model.ProjectID,
                ReviewerID = model.ReviewerID,
                RevieweeID = model.RevieweeID,
                Score = model.Score,
                Comment = model.Comment,
                DateRated = DateTime.Now,
                IsActive = true
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi đánh giá thành công.";
            return RedirectToAction("Details", "Project", new { id = model.ProjectID });
        }
    }
}

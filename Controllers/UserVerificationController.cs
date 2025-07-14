using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;

namespace StudentFreelance.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class UserVerificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserVerificationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: UserVerification
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => 
                    u.UserName.Contains(searchString) || 
                    u.Email.Contains(searchString) || 
                    u.FullName.Contains(searchString));
            }

            var users = await query
                .OrderBy(u => u.UserName)
                .Select(u => new UserVerificationViewModel
                {
                    UserID = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsVerified = u.IsVerified,
                    IsFlagged = u.IsFlagged,
                    FlagReason = u.FlagReason,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    StatusID = u.StatusID
                })
                .ToListAsync();

            return View(users);
        }

        // GET: UserVerification/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Get user account history
            var history = await _context.UserAccountActions
                .Where(h => h.UserID == id)
                .Include(h => h.ActionBy)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();

            var viewModel = new UserVerificationDetailsViewModel
            {
                User = user,
                History = history
            };

            return View(viewModel);
        }

        // POST: UserVerification/Verify/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if already verified
            if (user.IsVerified)
            {
                TempData["ErrorMessage"] = "User is already verified.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update user verification status
            user.IsVerified = true;
            user.VerifiedAt = DateTime.UtcNow;
            user.VerifiedByID = int.Parse(_userManager.GetUserId(User));

            // If user was flagged, unflag them
            if (user.IsFlagged)
            {
                user.IsFlagged = false;
                user.FlagReason = null;
                user.FlaggedAt = null;
                user.FlaggedByID = null;
            }

            // Log the action
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

            var action = new UserAccountAction
            {
                UserID = user.Id,
                ActionByID = int.Parse(_userManager.GetUserId(User)),
                ActionType = "Verify",
                Description = "User marked as verified",
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.UserAccountActions.Add(action);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User successfully verified.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: UserVerification/Flag/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Flag(int id, string reason)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if already flagged
            if (user.IsFlagged)
            {
                TempData["ErrorMessage"] = "User is already flagged.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update user flag status
            user.IsFlagged = true;
            user.FlagReason = reason;
            user.FlaggedAt = DateTime.UtcNow;
            user.FlaggedByID = int.Parse(_userManager.GetUserId(User));

            // If user was verified, unverify them
            if (user.IsVerified)
            {
                user.IsVerified = false;
                user.VerifiedAt = null;
                user.VerifiedByID = null;
            }

            // Log the action
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

            var action = new UserAccountAction
            {
                UserID = user.Id,
                ActionByID = int.Parse(_userManager.GetUserId(User)),
                ActionType = "Flag",
                Description = $"User flagged for: {reason}",
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.UserAccountActions.Add(action);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User successfully flagged.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: UserVerification/Unflag/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unflag(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if not flagged
            if (!user.IsFlagged)
            {
                TempData["ErrorMessage"] = "User is not flagged.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update user flag status
            user.IsFlagged = false;
            user.FlagReason = null;
            user.FlaggedAt = null;
            user.FlaggedByID = null;

            // Log the action
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

            var action = new UserAccountAction
            {
                UserID = user.Id,
                ActionByID = int.Parse(_userManager.GetUserId(User)),
                ActionType = "Unflag",
                Description = "User flag removed",
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.UserAccountActions.Add(action);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User flag successfully removed.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
} 
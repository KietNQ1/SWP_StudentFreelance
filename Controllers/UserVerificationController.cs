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
        public async Task<IActionResult> Index(string searchString, string roleFilter, string statusFilter, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["RoleFilter"] = roleFilter;
            ViewData["StatusFilter"] = statusFilter;
            
            // Đặt giá trị mặc định cho pageNumber
            int currentPageNumber = pageNumber ?? 1;
            int pageSize = 10;

            var query = _context.Users.AsQueryable();

            // Tìm kiếm theo tên, email, username
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => 
                    u.UserName.Contains(searchString) || 
                    u.Email.Contains(searchString) || 
                    u.FullName.Contains(searchString));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrEmpty(roleFilter))
            {
                switch (roleFilter.ToLower())
                {
                    case "student":
                        query = query.Where(u => u.University != null);
                        break;
                    case "business":
                        query = query.Where(u => u.CompanyName != null);
                        break;
                    case "admin":
                    case "moderator":
                        // Cần lấy danh sách người dùng theo vai trò từ UserManager
                        var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                        var userIds = usersInRole.Select(u => u.Id).ToList();
                        query = query.Where(u => userIds.Contains(u.Id));
                        break;
                }
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter.ToLower())
                {
                    case "active":
                        query = query.Where(u => u.IsActive);
                        break;
                    case "inactive":
                        query = query.Where(u => !u.IsActive);
                        break;
                    case "verified":
                        query = query.Where(u => u.IsVerified);
                        break;
                    case "flagged":
                        query = query.Where(u => u.IsFlagged);
                        break;
                }
            }

            // Đếm tổng số bản ghi để phân trang
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            
            // Lưu thông tin phân trang vào ViewData
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = currentPageNumber;
            ViewData["TotalRecords"] = totalRecords;

            // Lấy dữ liệu cho trang hiện tại
            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((currentPageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    StatusID = u.StatusID,
                    Phone = u.PhoneNumber
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
        
        // POST: UserVerification/Unverify/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unverify(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if not verified
            if (!user.IsVerified)
            {
                TempData["ErrorMessage"] = "User is not verified.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update user verification status
            user.IsVerified = false;
            user.VerifiedAt = null;
            user.VerifiedByID = null;

            // Log the action
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

            var action = new UserAccountAction
            {
                UserID = user.Id,
                ActionByID = int.Parse(_userManager.GetUserId(User)),
                ActionType = "Unverify",
                Description = "User verification removed",
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.UserAccountActions.Add(action);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User verification successfully removed.";
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
        
        // GET: UserVerification/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            // Soft delete - mark as inactive
            user.IsActive = false;
            
            // Log the action
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            
            var action = new UserAccountAction
            {
                UserID = user.Id,
                ActionByID = int.Parse(_userManager.GetUserId(User)),
                ActionType = "Delete",
                Description = "User marked as inactive",
                ActionDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };
            
            _context.UserAccountActions.Add(action);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "User successfully deleted (marked as inactive).";
            return RedirectToAction(nameof(Index));
        }
    }
} 
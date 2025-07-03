using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using StudentFreelance.ViewModels;
using Microsoft.AspNetCore.Identity;
using StudentFreelance.DbContext;

namespace StudentFreelance.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(user.Id);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                await _notificationService.MarkNotificationAsReadAsync(id, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MarkAsRead Error: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                await _notificationService.MarkAllNotificationsAsReadAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MarkAllAsRead Error: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Send()
        {
            var model = new SendNotificationViewModel
            {
                AllUsers = await _userManager.Users
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                    .ToListAsync(),

                NotificationTypes = await _context.NotificationTypes
                    .Select(t => new SelectListItem { Value = t.TypeID.ToString(), Text = t.TypeName })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Send(SendNotificationViewModel model)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (model.IsBroadcast && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError("", "Chỉ Admin mới được phép gửi thông báo toàn hệ thống.");
            }

            if (!model.IsBroadcast && (model.SelectedUserIDs == null || !model.SelectedUserIDs.Any()))
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một người nhận.");
            }

            if (!ModelState.IsValid)
            {
                model.AllUsers = await _userManager.Users
                    .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                    .ToListAsync();

                model.NotificationTypes = await _context.NotificationTypes
                    .Select(t => new SelectListItem { Value = t.TypeID.ToString(), Text = t.TypeName })
                    .ToListAsync();

                return View(model);
            }

            try
            {
                if (model.IsBroadcast)
                {
                    _logger.LogInformation($"[Broadcast] {sender.FullName} gửi thông báo cho toàn hệ thống.");
                    await _notificationService.SendBroadcastNotificationAsync(model.Title, model.Content, model.TypeID, null, sender.Id);
                }
                else
                {
                    _logger.LogInformation($"[MultiSend] {sender.FullName} gửi cho {model.SelectedUserIDs.Count} người.");
                    await _notificationService.SendNotificationToMultipleUsersAsync(
                        model.SelectedUserIDs, model.Title, model.Content, model.TypeID, null, sender.Id);
                }

                TempData["Success"] = "Gửi thông báo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Send Error] {ex.Message}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi thông báo.");
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var notification = await _notificationService.GetNotificationByIdAsync(id);

            if (notification == null)
                return NotFound();

            // Kiểm tra quyền truy cập
            bool isRecipient = await _context.UserNotifications
                .AnyAsync(un => un.NotificationID == id && un.UserID == user.Id);

            if (!notification.IsBroadcast && !isRecipient)
            {
                return Forbid(); // Tránh xem thông báo không thuộc về mình
            }

            // Tự động đánh dấu đã đọc nếu chưa
            await _notificationService.MarkNotificationAsReadAsync(id, user.Id);

            return View(notification);
        }

    }
}

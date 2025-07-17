using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public NotificationService(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // CRUD operations
        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications
                .Include(n => n.Type)
                .Include(n => n.Sender)
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.Type)
                .Include(n => n.Sender)
                .Include(n => n.UserNotifications)
                    .ThenInclude(un => un.User)
                .FirstOrDefaultAsync(n => n.NotificationID == id && n.IsActive);
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.NotificationDate = DateTime.Now;
            notification.IsActive = true;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            notification.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // User-specific notifications
        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Include(n => n.Type)
                .Include(n => n.Sender)
                .Include(n => n.UserNotifications.Where(un => un.UserID == userId))
                .Where(n => (n.UserNotifications.Any(un => un.UserID == userId) || n.IsBroadcast) && n.IsActive)
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Include(n => n.Type)
                .Include(n => n.Sender)
                .Include(n => n.UserNotifications.Where(un => un.UserID == userId))
                .Where(n => n.IsActive &&
                    (
                        (n.UserNotifications.Any(un => un.UserID == userId && !un.IsRead)) ||
                        (n.IsBroadcast && !n.UserNotifications.Any(un => un.UserID == userId))
                    ))
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .Include(n => n.UserNotifications)
                .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.IsActive);

            if (notification == null)
                return false;

            var userNotification = notification.UserNotifications.FirstOrDefault(un => un.UserID == userId);

            if (userNotification == null)
            {
                userNotification = new UserNotification
                {
                    UserID = userId,
                    NotificationID = notificationId,
                    IsRead = true,
                    ReadDate = DateTime.Now
                };
                _context.UserNotifications.Add(userNotification);
            }
            else
            {
                userNotification.IsRead = true;
                userNotification.ReadDate = DateTime.Now;
                _context.UserNotifications.Update(userNotification);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int userId)
        {
            try
            {
                var userNotifications = await _context.UserNotifications
                    .Where(un => un.UserID == userId && !un.IsRead)
                    .ToListAsync();

                foreach (var un in userNotifications)
                {
                    un.IsRead = true;
                    un.ReadDate = DateTime.Now;
                }

                var unreadBroadcasts = await _context.Notifications
                    .Where(n => n.IsBroadcast && n.IsActive)
                    .Where(n => !n.UserNotifications.Any(un => un.UserID == userId))
                    .ToListAsync();

                foreach (var broadcast in unreadBroadcasts)
                {
                    _context.UserNotifications.Add(new UserNotification
                    {
                        UserID = userId,
                        NotificationID = broadcast.NotificationID,
                        IsRead = true,
                        ReadDate = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Send notifications
        public async Task<bool> SendNotificationToUserAsync(int userId, string title, string content, int typeId, int? relatedId = null, int? senderId = null, bool sendEmail = false)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    TypeID = typeId,
                    RelatedID = relatedId,
                    SenderID = senderId,
                    NotificationDate = DateTime.Now,
                    IsBroadcast = false,
                    IsActive = true
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var userNotification = new UserNotification
                {
                    UserID = userId,
                    NotificationID = notification.NotificationID,
                    IsRead = false
                };

                _context.UserNotifications.Add(userNotification);

                if (sendEmail)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user?.EmailConfirmed == true && !string.IsNullOrEmpty(user.Email))
                    {
                        await _emailSender.SendEmailAsync(user.Email, title, content);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendNotificationToMultipleUsersAsync(List<int> userIds, string title, string content, int typeId, int? relatedId = null, int? senderId = null, bool sendEmail = false)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    TypeID = typeId,
                    RelatedID = relatedId,
                    SenderID = senderId,
                    NotificationDate = DateTime.Now,
                    IsBroadcast = false,
                    IsActive = true
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var userNotifications = userIds.Select(userId => new UserNotification
                {
                    UserID = userId,
                    NotificationID = notification.NotificationID,
                    IsRead = false
                }).ToList();

                _context.UserNotifications.AddRange(userNotifications);

                if (sendEmail)
                {
                    var users = await _context.Users
                        .Where(u => userIds.Contains(u.Id) && u.EmailConfirmed && !string.IsNullOrEmpty(u.Email))
                        .ToListAsync();

                    foreach (var user in users)
                    {
                        await _emailSender.SendEmailAsync(user.Email, title, content);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendBroadcastNotificationAsync(string title, string content, int typeId, int? relatedId = null, int? senderId = null, bool sendEmail = false)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    TypeID = typeId,
                    RelatedID = relatedId,
                    SenderID = senderId,
                    NotificationDate = DateTime.Now,
                    IsBroadcast = true,
                    IsActive = true
                };

                _context.Notifications.Add(notification);

                if (sendEmail)
                {
                    var users = await _context.Users
                        .Where(u => u.EmailConfirmed && !string.IsNullOrEmpty(u.Email))
                        .ToListAsync();

                    foreach (var user in users)
                    {
                        await _emailSender.SendEmailAsync(user.Email, title, content);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

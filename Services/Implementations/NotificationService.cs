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

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
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
            {
                return false;
            }

            notification.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // User-specific notifications
        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            // Get both direct notifications and broadcast notifications
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
            // Combine unread user-specific notifications with unread broadcast notifications
            // For broadcasts, check if there's no UserNotification entry or if it exists but IsRead is false
            return await _context.Notifications
                .Include(n => n.Type)
                .Include(n => n.Sender)
                .Include(n => n.UserNotifications.Where(un => un.UserID == userId))
                .Where(n => n.IsActive && 
                    (
                        // User-specific unread notifications
                        (n.UserNotifications.Any(un => un.UserID == userId && !un.IsRead)) ||
                        // Broadcast notifications that user hasn't marked as read
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

            // Check if this is a broadcast notification without a UserNotification entry yet
            var userNotification = notification.UserNotifications.FirstOrDefault(un => un.UserID == userId);
            
            if (userNotification == null)
            {
                // Create a new UserNotification entry for this broadcast notification
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
                // Update existing UserNotification
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
                // Get all user's unread notifications (including broadcasts)
                var userNotifications = await _context.UserNotifications
                    .Where(un => un.UserID == userId && !un.IsRead)
                    .ToListAsync();
                
                // Mark existing UserNotifications as read
                foreach (var un in userNotifications)
                {
                    un.IsRead = true;
                    un.ReadDate = DateTime.Now;
                }
                
                // Get broadcast notifications that user hasn't interacted with yet
                var unreadBroadcasts = await _context.Notifications
                    .Where(n => n.IsBroadcast && n.IsActive)
                    .Where(n => !n.UserNotifications.Any(un => un.UserID == userId))
                    .ToListAsync();
                
                // Create new UserNotification entries for broadcasts
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
            catch (Exception)
            {
                return false;
            }
        }

        // Send notifications methods
        public async Task<bool> SendNotificationToUserAsync(int userId, string title, string content, int typeId, int? relatedId = null, int? senderId = null)
        {
            try
            {
                // Create notification
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
                
                // Create UserNotification link
                var userNotification = new UserNotification
                {
                    UserID = userId,
                    NotificationID = notification.NotificationID,
                    IsRead = false
                };
                
                _context.UserNotifications.Add(userNotification);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendNotificationToMultipleUsersAsync(List<int> userIds, string title, string content, int typeId, int? relatedId = null, int? senderId = null)
        {
            try
            {
                // Create notification
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
                
                // Create UserNotification links for each user
                var userNotifications = userIds.Select(userId => new UserNotification
                {
                    UserID = userId,
                    NotificationID = notification.NotificationID,
                    IsRead = false
                }).ToList();
                
                _context.UserNotifications.AddRange(userNotifications);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendBroadcastNotificationAsync(string title, string content, int typeId, int? relatedId = null, int? senderId = null)
        {
            try
            {
                // Create broadcast notification
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
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 
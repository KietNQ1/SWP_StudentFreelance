using StudentFreelance.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
{
    public interface INotificationService
    {
        // Notification CRUD
        Task<IEnumerable<Notification>> GetAllNotificationsAsync();
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(int id);

        // User-specific notifications
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllNotificationsAsReadAsync(int userId);

        // Send notifications methods
        Task<bool> SendNotificationToUserAsync(int userId, string title, string content, int typeId, int? relatedId = null, int? senderId = null, bool sendEmail = false);
        Task<bool> SendNotificationToMultipleUsersAsync(List<int> userIds, string title, string content, int typeId, int? relatedId = null, int? senderId = null, bool sendEmail = false);
        Task<bool> SendBroadcastNotificationAsync(string title, string content, int typeId, int? relatedId = null, int? senderId = null, bool sendEmail = false);
        Task SendNotificationToAdminAsync(string title, string content);
    }
}

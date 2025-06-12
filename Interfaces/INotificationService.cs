using StudentFreelance.Models;

namespace StudentFreelance.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllNotificationsAsync();
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(int id);
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId);
        Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(int typeId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    }
} 
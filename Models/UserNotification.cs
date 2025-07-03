using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class UserNotification
    {
        [Key]
        public int UserNotificationID { get; set; }

        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        [ForeignKey(nameof(Notification))]
        public int NotificationID { get; set; }

        /// Cờ đã đọc chưa
        public bool IsRead { get; set; } = false;

        /// Thời gian đã đọc
        public DateTime? ReadDate { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Notification Notification { get; set; }
    }
} 
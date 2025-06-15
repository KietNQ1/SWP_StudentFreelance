using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class Notification
    {
        /// Khóa chính, tự tăng
        [Key]
        public int NotificationID { get; set; }

        /// FK → User.UserID (người nhận)
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        /// Tiêu đề thông báo
        [Required, StringLength(200)]
        public string Title { get; set; }

        /// Nội dung thông báo
        public string Content { get; set; }

        /// Loại thông báo
        [ForeignKey(nameof(Type))]
        public int TypeID { get; set; }

        /// ID liên quan (ProjectID, MessageID,…)
        public int? RelatedID { get; set; }

        /// Ngày giờ tạo thông báo
        public DateTime NotificationDate { get; set; }

        /// Cờ đã đọc chưa
        public bool IsRead { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ApplicationUser User { get; set; }
        public NotificationType Type { get; set; }
    }
}

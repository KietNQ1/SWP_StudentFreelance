using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Message
    {
        /// Khóa chính, tự tăng
        [Key]
        public int MessageID { get; set; }

        /// FK → User.UserID (người gửi)
        [ForeignKey(nameof(Sender))]
        public int SenderID { get; set; }

        /// FK → User.UserID (người nhận)
        [ForeignKey(nameof(Receiver))]
        public int ReceiverID { get; set; }

        /// FK → Project.ProjectID (nullable)
        [ForeignKey(nameof(Project))]
        public int? ProjectID { get; set; }

        /// Nội dung tin nhắn
        [Required]
        public string Content { get; set; }

        /// Cờ đã đọc chưa
        public bool IsRead { get; set; }

        /// Ngày giờ gửi
        public DateTime SentAt { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public User Sender { get; set; }
        public User Receiver { get; set; }
        public Project Project { get; set; }
    }
}

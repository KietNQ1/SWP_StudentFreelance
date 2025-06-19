using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class UserAccountHistory
    {
        /// Khóa chính, tự tăng
        [Key]
        public int HistoryID { get; set; }

        /// FK → User.UserID
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        /// Loại hành động (UPDATE_PROFILE, CHANGE_PASSWORD,…)
        [Required]
        public string ActionType { get; set; }

        /// Mô tả chi tiết hành động
        public string Description { get; set; }

        /// Giá trị cũ trước khi thay đổi
        public string OldValue { get; set; }

        /// Giá trị mới sau khi thay đổi
        public string NewValue { get; set; }

        /// Ngày giờ thực hiện hành động
        public DateTime ActionDate { get; set; }

        /// Địa chỉ IP khi thực hiện
        public string IPAddress { get; set; }

        /// UserAgent của client
        public string UserAgent { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public ApplicationUser User { get; set; }
    }
}

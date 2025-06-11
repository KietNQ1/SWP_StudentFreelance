using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Transaction
    {
        /// Khóa chính, tự tăng
        [Key]
        public int TransactionID { get; set; }

        /// FK → User.UserID
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        /// FK → Project.ProjectID (nullable)
        [ForeignKey(nameof(Project))]
        public int? ProjectID { get; set; }

        /// Số tiền (decimal(15,2))
        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        /// Loại giao dịch (Payment, Refund,…)
        [Required]
        public string TransactionType { get; set; }

        /// Ngày giờ giao dịch
        public DateTime TransactionDate { get; set; }

        /// Diễn giải giao dịch
        public string Description { get; set; }

        /// Trạng thái (Completed, Failed,…)
        [Required]
        public string Status { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public User User { get; set; }
        public Project Project { get; set; }
    }
}

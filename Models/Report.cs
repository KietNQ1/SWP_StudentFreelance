using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Report
    {
        /// Khóa chính, tự tăng
        [Key]
        public int ReportID { get; set; }

        /// FK → User.UserID (người báo cáo)
        [ForeignKey(nameof(Reporter))]
        public int ReporterID { get; set; }

        /// FK → User.UserID (người bị báo cáo)
        [ForeignKey(nameof(ReportedUser))]
        public int ReportedUserID { get; set; }

        /// FK → Project.ProjectID (nullable)
        [ForeignKey(nameof(Project))]
        public int? ProjectID { get; set; }

        /// Loại báo cáo (Spam, Fraud,…)
        [Required]
        public string ReportType { get; set; }

        /// Trạng thái xử lý (Pending, Resolved,…)
        [Required]
        public string Status { get; set; }

        /// Ngày giờ tạo báo cáo
        public DateTime ReportDate { get; set; }

        /// Nội dung báo cáo
        [Required]
        public string Description { get; set; }

        /// Phản hồi của admin
        public string AdminResponse { get; set; }

        /// Ngày giờ hoàn thành xử lý (nullable)
        public DateTime? ResolvedAt { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public User Reporter { get; set; }
        public User ReportedUser { get; set; }
        public Project Project { get; set; }
    }
}

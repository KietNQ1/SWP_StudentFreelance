using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

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

        /// Ngày giờ tạo báo cáo
        public DateTime ReportDate { get; set; }

        /// FK → ReportType.TypeID (nullable)
        [ForeignKey(nameof(Type))]
        public int TypeID { get; set; }

        /// Nội dung báo cáo
        [Required]
        public string Description { get; set; }

        /// FK → ReportStatus.StatusID (nullable)
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }

        /// Ngày giờ hoàn thành xử lý (nullable)
        public DateTime? ResolvedAt { get; set; }

        /// Phản hồi của admin (nullable)
        public string? AdminResponse { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public ApplicationUser Reporter { get; set; }
        public ApplicationUser ReportedUser { get; set; }
        public Project Project { get; set; }
        public ReportType Type { get; set; }
        public ReportStatus Status { get; set; }
    }
}

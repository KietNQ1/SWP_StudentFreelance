using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class StudentApplication
    {
        /// Khóa chính, tự tăng
        [Key]
        public int ApplicationID { get; set; }

        /// FK → Project.ProjectID
        [ForeignKey(nameof(Project))]
        public int ProjectID { get; set; }

        /// FK → User.UserID
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        /// Thư xin ứng tuyển
        [Required]
        public string CoverLetter { get; set; }

        /// Mức lương đề xuất (decimal(15,2))
        [Column(TypeName = "decimal(15,2)")]
        public decimal Salary { get; set; }

        /// Trạng thái đơn (Pending, Accepted, Rejected,…)
        [Required]
        public string Status { get; set; }

        /// Ngày gửi đơn
        public DateTime DateApplied { get; set; }

        /// Ngày cập nhật trạng thái gần nhất
        public DateTime? LastStatusUpdate { get; set; }

        /// Ghi chú của doanh nghiệp về đơn ứng tuyển
        public string? BusinessNotes { get; set; }

        /// Đường dẫn đến CV đính kèm (nếu có)
        [Display(Name = "Đường dẫn đến CV")]
        public string? ResumeAttachment { get; set; }

        /// Đường dẫn đến portfolio đính kèm (nếu có)
        [Display(Name = "Đường dẫn đến portfolio")]
        public string? PortfolioLink { get; set; }

        /// Điểm đánh giá của doanh nghiệp cho đơn ứng tuyển (1-5)
        public int? BusinessRating { get; set; }

        /// Thời gian phỏng vấn (nếu được lên lịch)
        public DateTime? InterviewSchedule { get; set; }

        /// Kết quả phỏng vấn
        public string? InterviewResult { get; set; }

        public Project Project { get; set; }
        public ApplicationUser User { get; set; }
    }
}

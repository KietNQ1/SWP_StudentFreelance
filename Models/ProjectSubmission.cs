using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class ProjectSubmission
    {
        /// Khóa chính, tự tăng
        [Key]
        public int SubmissionID { get; set; }

        /// FK → StudentApplication.ApplicationID
        [ForeignKey(nameof(Application))]
        public int ApplicationID { get; set; }

        /// Tiêu đề nộp
        [Required, StringLength(200)]
        public string Title { get; set; }

        /// Mô tả chi tiết về kết quả dự án
        [Required]
        public string Description { get; set; }

        /// Ngày nộp
        public DateTime SubmittedAt { get; set; }

        /// Trạng thái nộp (Pending, Approved, Rejected)
        [Required]
        public string Status { get; set; }

        /// Phản hồi từ doanh nghiệp
        public string? BusinessFeedback { get; set; }

        /// Ngày phản hồi
        public DateTime? FeedbackDate { get; set; }

        /// Cờ kích hoạt
        public bool IsActive { get; set; } = true;

        /// Navigation properties
        public StudentApplication Application { get; set; }
        
        /// Danh sách các file đính kèm
        public ICollection<ProjectSubmissionAttachment> Attachments { get; set; } = new List<ProjectSubmissionAttachment>();
    }
} 
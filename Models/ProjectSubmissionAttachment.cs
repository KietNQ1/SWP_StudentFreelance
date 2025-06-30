using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class ProjectSubmissionAttachment
    {
        /// Khóa chính, tự tăng
        [Key]
        public int AttachmentID { get; set; }

        /// FK → ProjectSubmission.SubmissionID
        [ForeignKey(nameof(Submission))]
        public int SubmissionID { get; set; }

        /// Tên file gốc
        [Required]
        public string FileName { get; set; }

        /// Đường dẫn lưu file
        [Required]
        public string FilePath { get; set; }

        /// Kích thước file (bytes)
        public long FileSize { get; set; }

        /// MIME type (image/png,…)
        public string ContentType { get; set; }

        /// Mô tả file đính kèm
        public string Description { get; set; }

        /// Ngày giờ upload
        public DateTime UploadedAt { get; set; }

        /// FK → User.UserID (người upload)
        [ForeignKey(nameof(UploadedByUser))]
        public int UploadedBy { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public ProjectSubmission Submission { get; set; }
        public ApplicationUser UploadedByUser { get; set; }
    }
} 
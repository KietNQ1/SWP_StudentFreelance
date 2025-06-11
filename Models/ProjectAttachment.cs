using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class ProjectAttachment
    {
        /// Khóa chính, tự tăng
        [Key]
        public int AttachmentID { get; set; }

        /// FK → Project.ProjectID
        [ForeignKey(nameof(Project))]
        public int ProjectID { get; set; }

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

        public Project Project { get; set; }
        public User UploadedByUser { get; set; }
    }
}

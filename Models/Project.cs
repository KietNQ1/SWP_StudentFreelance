using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class Project
    {
        /// Khóa chính, tự tăng
        [Key]
        public int ProjectID { get; set; }

        /// FK → User.UserID (business)
        [ForeignKey(nameof(Business))]
        public int BusinessID { get; set; }

        /// FK → Category.CategoryID
        [ForeignKey(nameof(Category))]
        public int CategoryID { get; set; }

        /// Tiêu đề dự án (nvarchar(200))
        [Required, StringLength(200)]
        public string Title { get; set; }

        /// Mô tả chi tiết
        public string Description { get; set; }

        /// Ngân sách (decimal(15,2))
        [Column(TypeName = "decimal(15,2)")]
        public decimal Budget { get; set; }

        /// Hạn nộp (deadline)
        public DateTime Deadline { get; set; }

        /// Trạng thái (Open, Closed,…)
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }

        /// Cờ nổi bật
        public bool IsHighlighted { get; set; }

        /// Loại hình công việc (Full-time, Part-time,…)
        [ForeignKey(nameof(Type))]
        public int TypeID { get; set; }

        /// FK → Address.AddressID (nullable)
        [ForeignKey(nameof(Address))]
        public int? AddressID { get; set; }

        /// Cờ remote
        public bool IsRemoteWork { get; set; }

        /// Ngày tạo bản ghi
        public DateTime CreatedAt { get; set; }

        /// Ngày cập nhật gần nhất
        public DateTime UpdatedAt { get; set; }

        /// Ngày bắt đầu
        public DateTime StartDate { get; set; }

        /// Ngày kết thúc
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public User Business { get; set; }
        public Category Category { get; set; }
        public Address Address { get; set; }
        public ProjectStatus Status { get; set; }
        public ProjectType Type { get; set; }
        public ICollection<ProjectSkillRequired> ProjectSkillsRequired { get; set; } = new List<ProjectSkillRequired>();
        public ICollection<ProjectAttachment> ProjectAttachments { get; set; } = new List<ProjectAttachment>();
        public ICollection<StudentApplication> StudentApplications { get; set; } = new List<StudentApplication>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}

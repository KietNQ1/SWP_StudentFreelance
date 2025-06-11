using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string CoverLetter { get; set; }

        /// Mức lương đề xuất (decimal(15,2))
        [Column(TypeName = "decimal(15,2)")]
        public decimal Salary { get; set; }

        /// Trạng thái đơn (Pending, Accepted, Rejected,…)
        [Required]
        public string Status { get; set; }

        /// Ngày gửi đơn
        public DateTime DateApplied { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public Project Project { get; set; }
        public User User { get; set; }
    }
}

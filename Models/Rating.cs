using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Rating
    {
        /// Khóa chính, tự tăng
        [Key]
        public int RatingID { get; set; }

        /// FK → Project.ProjectID
        [ForeignKey(nameof(Project))]
        public int ProjectID { get; set; }

        /// FK → User.UserID (người chấm)
        [ForeignKey(nameof(Reviewer))]
        public int ReviewerID { get; set; }

        /// FK → User.UserID (người được chấm)
        [ForeignKey(nameof(Reviewee))]
        public int RevieweeID { get; set; }

        /// Điểm (decimal(3,2))
        [Column(TypeName = "decimal(3,2)")]
        public decimal Score { get; set; }

        /// Nhận xét
        public string Comment { get; set; }

        /// Ngày giờ chấm điểm
        public DateTime DateRated { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public Project Project { get; set; }
        public ApplicationUser Reviewer { get; set; }
        public ApplicationUser Reviewee { get; set; }
    }
}

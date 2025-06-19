using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class Skill
    {
        /// Khóa chính, tự tăng
        [Key]
        public int SkillID { get; set; }

        /// Tên kỹ năng (nvarchar(100))
        [Required, StringLength(100)]
        public string SkillName { get; set; }

        /// FK → Category.CategoryID
        [ForeignKey(nameof(Category))]
        public int CategoryID { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public Category Category { get; set; }
        public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
        public ICollection<ProjectSkillRequired> ProjectSkillsRequired { get; set; } = new List<ProjectSkillRequired>();
    }
}

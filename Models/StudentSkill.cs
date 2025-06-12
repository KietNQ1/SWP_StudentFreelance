using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class StudentSkill
    {
        /// Khóa chính, tự tăng
        [Key]
        public int StudentSkillID { get; set; }

        /// FK → User.UserID
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        /// FK → Skill.SkillID
        [ForeignKey(nameof(Skill))]
        public int SkillID { get; set; }

        /// FK → ProficiencyLevel.ProficiencyLevelID
        [ForeignKey(nameof(ProficiencyLevel))]
        public int ProficiencyLevelID { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public User User { get; set; }
        public Skill Skill { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
    }
}

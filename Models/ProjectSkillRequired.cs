using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class ProjectSkillRequired
    {
        /// Khóa chính, tự tăng
        [Key]
        public int ProjectSkillID { get; set; }

        /// FK → Project.ProjectID
        [ForeignKey(nameof(Project))]
        public int ProjectID { get; set; }

        /// FK → Skill.SkillID
        [ForeignKey(nameof(Skill))]
        public int SkillID { get; set; }

        /// FK → ImportanceLevel.ImportanceLevelID
        [ForeignKey(nameof(ImportanceLevel))]
        public int ImportanceLevelID { get; set; }

        /// Cờ kích hoạt (true nếu bản ghi còn hiệu lực, false nếu inactive)
        public bool IsActive { get; set; } = true;

        public Project Project { get; set; }
        public Skill Skill { get; set; }
        public ImportanceLevel ImportanceLevel { get; set; }
    }
}

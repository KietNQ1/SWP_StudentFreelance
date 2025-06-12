using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class ImportanceLevel
    {
        [Key]
        public int LevelID { get; set; }

        [Required]
        public required string LevelName { get; set; }

        public bool IsActive { get; set; }
        
        // Navigation property
        public ICollection<ProjectSkillRequired> ProjectSkillsRequired { get; set; } = new List<ProjectSkillRequired>();
    }
} 
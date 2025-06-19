using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class ProficiencyLevel
    {
        [Key]
        public int LevelID { get; set; }

        [Required]
        public required string LevelName { get; set; }

        public bool IsActive { get; set; }
        
        // Navigation property
        public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
    }
} 
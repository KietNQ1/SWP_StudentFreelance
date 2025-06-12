using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class ProjectStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        public required string StatusName { get; set; }

        public bool IsActive { get; set; }
        
        // Navigation property
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
} 
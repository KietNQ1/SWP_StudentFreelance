using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class ProjectFlagAction
    {
        [Key]
        public int ActionID { get; set; }
        
        [ForeignKey(nameof(Project))]
        public int ProjectID { get; set; }
        
        [ForeignKey(nameof(ActionBy))]
        public int ActionByID { get; set; }
        
        [Required]
        public string ActionType { get; set; } // "Flag", "Unflag", etc.
        
        public string Reason { get; set; }
        
        [Required]
        public DateTime ActionDate { get; set; }
        
        [Required]
        public string IPAddress { get; set; }
        
        [Required]
        public string UserAgent { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public Project Project { get; set; }
        public ApplicationUser ActionBy { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class UserAccountAction
    {
        [Key]
        public int ActionID { get; set; }
        
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }
        
        [ForeignKey(nameof(ActionBy))]
        public int ActionByID { get; set; }
        
        [Required]
        public string ActionType { get; set; } // "Verify", "Flag", "Unflag", etc.
        
        public string Description { get; set; }
        
        [Required]
        public DateTime ActionDate { get; set; }
        
        [Required]
        public string IPAddress { get; set; }
        
        [Required]
        public string UserAgent { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ApplicationUser User { get; set; }
        public ApplicationUser ActionBy { get; set; }
    }
} 
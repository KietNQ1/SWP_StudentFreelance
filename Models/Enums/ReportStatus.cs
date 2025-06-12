using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class ReportStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        public required string StatusName { get; set; }

        public bool IsActive { get; set; }
        
        // Navigation property
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
} 
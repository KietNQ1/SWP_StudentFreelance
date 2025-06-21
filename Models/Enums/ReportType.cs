using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class ReportType
    {
        [Key]
        public int TypeID { get; set; }

        [Required]
        public required string TypeName { get; set; }

        public bool IsActive { get; set; }
        
        // Navigation property
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
} 
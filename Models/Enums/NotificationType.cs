using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class NotificationType
    {
        [Key]
        public int TypeID { get; set; }

        [Required]
        public required string TypeName { get; set; }

        public bool IsActive { get; set; }
        
        // Navigation property
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
} 
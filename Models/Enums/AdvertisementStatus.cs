using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class AdvertisementStatus
    {
        [Key]
        public int StatusID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 
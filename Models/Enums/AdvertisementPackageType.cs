using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.Models.Enums
{
    public class AdvertisementPackageType
    {
        [Key]
        public int PackageTypeID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PackageTypeName { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        public int DurationDays { get; set; } = 7;
        
        public bool IsActive { get; set; } = true;
    }
} 
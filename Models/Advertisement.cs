using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.Models
{
    public class Advertisement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BusinessId { get; set; }
        
        [Required]
        [ForeignKey("BusinessId")]
        public ApplicationUser Business { get; set; }
        
        [Required]
        public string ImagePath { get; set; }
        
        [Required]
        public int PackageTypeID { get; set; }
        
        [ForeignKey("PackageTypeID")]
        public AdvertisementPackageType PackageType { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public int StatusID { get; set; } = 1; // Default: Pending
        
        [ForeignKey("StatusID")]
        public AdvertisementStatus Status { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public int? ApprovedById { get; set; }
        
        [ForeignKey("ApprovedById")]
        public ApplicationUser ApprovedBy { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 
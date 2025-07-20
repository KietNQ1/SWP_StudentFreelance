using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace StudentFreelance.ViewModels
{
    public class AdvertisementViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please select an advertisement package type")]
        [Display(Name = "Package Type")]
        public int PackageTypeID { get; set; }
        
        [Display(Name = "Package Name")]
        public string PackageTypeName { get; set; }
        
        [Display(Name = "Advertisement Image")]
        public string ImagePath { get; set; }
        
        [Display(Name = "Upload Advertisement Image")]
        public IFormFile ImageFile { get; set; }
        
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Status")]
        public int StatusID { get; set; } = 1; // Default: Pending
        
        [Display(Name = "Status Name")]
        public string StatusName { get; set; }
        
        [Display(Name = "Approval Date")]
        public DateTime? ApprovedAt { get; set; }
        
        [Display(Name = "Approved By")]
        public string ApprovedByName { get; set; }
        
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }
        
        [Display(Name = "Price")]
        public decimal Price { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Compatibility properties for existing views
        [Display(Name = "Is Approved")]
        public bool IsApproved => StatusID == 2; // 2 = Approved
    }
    
    public class AdvertisementListViewModel
    {
        public List<AdvertisementViewModel> Advertisements { get; set; }
        public bool ShowApprovalOptions { get; set; }
    }
} 
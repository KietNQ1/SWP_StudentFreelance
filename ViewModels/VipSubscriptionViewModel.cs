using System;
using System.Collections.Generic;

namespace StudentFreelance.ViewModels
{
    public class VipSubscriptionViewModel
    {
        public int RoleId { get; set; } // 3 for Business, 4 for Student
        public decimal WalletBalance { get; set; }
        public bool IsVip { get; set; }
        public DateTime? VipExpiryDate { get; set; }
        public List<VipPlanOption> Plans { get; set; } = new List<VipPlanOption>();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }

    public class VipPlanOption
    {
        public int Months { get; set; }
        public decimal Price { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public string Description { get; set; }
    }
} 
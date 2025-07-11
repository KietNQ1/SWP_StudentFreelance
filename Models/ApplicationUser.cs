using Microsoft.AspNetCore.Identity;
using StudentFreelance.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace StudentFreelance.Models
{
    

    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; }
        
        public bool VipStatus { get; set; }
        public int? AddressID { get; set; }
        public decimal WalletBalance { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public string? CompanyName { get; set; }
        public string? Industry { get; set; }
        public bool ProfileStatus { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalProjects { get; set; }
        public int TotalProjectsPosted { get; set; }
        public string? ProfilePicturePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; } = true;
        public int StatusID { get; set; }

        // Navigation properties

        public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

        [ForeignKey(nameof(AddressID))]
        public Address Address { get; set; }

        [ForeignKey(nameof(StatusID))]
        public AccountStatus Status { get; set; }
        
        // Notification collections
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    }
}

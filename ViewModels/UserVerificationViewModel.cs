using System;

namespace StudentFreelance.ViewModels
{
    public class UserVerificationViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsVerified { get; set; }
        public bool IsFlagged { get; set; }
        public string FlagReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int StatusID { get; set; }
        public string Phone { get; set; }
    }
} 
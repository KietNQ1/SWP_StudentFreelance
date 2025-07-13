using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class UserVerificationDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<UserAccountAction> History { get; set; }
    }
} 
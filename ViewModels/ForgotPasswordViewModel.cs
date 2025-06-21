using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

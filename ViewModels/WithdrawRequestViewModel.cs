using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace StudentFreelance.ViewModels
{
    public class WithdrawRequestViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Withdrawal amount is required.")]
        [Range(10000, 100000000, ErrorMessage = "Withdrawal amount must be between 10,000 and 100,000,000 VND.")]
        public decimal Amount { get; set; }

        [StringLength(255, ErrorMessage = "Description must be less than 255 characters.")]
        public string? Description { get; set; }

        public bool UseBankAccount { get; set; }

        public string? BankName { get; set; }

        public string? AccountNumber { get; set; }

        public string? AccountHolderName { get; set; }

        public IFormFile? QRCodeImage { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UseBankAccount)
            {
                if (string.IsNullOrWhiteSpace(BankName) || !BankName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                    yield return new ValidationResult("Bank name is required and must contain only letters and spaces.", new[] { nameof(BankName) });

                if (string.IsNullOrWhiteSpace(AccountNumber) || !AccountNumber.All(char.IsDigit) || AccountNumber.Length < 8 || AccountNumber.Length > 16)
                    yield return new ValidationResult("Account number must be 8 to 16 digits.", new[] { nameof(AccountNumber) });

                if (string.IsNullOrWhiteSpace(AccountHolderName) || !AccountHolderName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                    yield return new ValidationResult("Account holder name is required and must contain only letters and spaces.", new[] { nameof(AccountHolderName) });
            }
            else
            {
                if (QRCodeImage == null)
                    yield return new ValidationResult("Please upload a QR code image if you are not using a bank account.", new[] { nameof(QRCodeImage) });
            }
        }
    }
}

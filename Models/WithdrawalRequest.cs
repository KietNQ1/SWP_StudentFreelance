using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class WithdrawalRequest
    {
        public int WithdrawalRequestID { get; set; }

        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public ApplicationUser User { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        // Nếu dùng BankAccount
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }

        // Nếu dùng QR
        public string? QRCodeUrl { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessedAt { get; set; }
    }
}

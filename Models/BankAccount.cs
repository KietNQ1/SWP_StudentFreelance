using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFreelance.Models
{
    public class BankAccount
    {
        public int Id { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; }

        [Required]
        [MaxLength(30)]
        public string AccountNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountHolderName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

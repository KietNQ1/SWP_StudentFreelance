using StudentFreelance.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.ViewModels
{
    public class WalletViewModel
    {
        public decimal WalletBalance { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }

    public class DepositViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0 and less than 1,000,000")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string Description { get; set; }
    }

    public class WithdrawViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0 and less than 1,000,000")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters")]
        public string Description { get; set; }
    }
} 
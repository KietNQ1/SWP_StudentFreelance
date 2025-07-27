using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding; // <-- Thêm namespace này để dùng [BindNever]

namespace StudentFreelance.ViewModels
{
    public class CreateRatingViewModel
    {
        public int ProjectID { get; set; }
        public int ReviewerID { get; set; }
        public int RevieweeID { get; set; }

        [Range(1, 5)]
        public decimal Score { get; set; }

        [Required]
        [MinLength(10)]
        public string Comment { get; set; }

        [BindNever] // Không cần gửi từ form
        public string ?ProjectTitle { get; set; }

        [BindNever] // Không cần gửi từ form
        public string ?RevieweeName { get; set; }

        public int ?ApplicationId { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.ViewModels
{
    public class SendNotificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        public string Content { get; set; }

        [Display(Name = "Loại thông báo")]
        [Required(ErrorMessage = "Vui lòng chọn loại.")]
        public int TypeID { get; set; }

        [Display(Name = "Gửi cho toàn bộ người dùng?")]
        public bool IsBroadcast { get; set; }

        [Display(Name = "Người nhận")]
        public List<int> SelectedUserIDs { get; set; } = new();

        // Dropdown nguồn
        public List<SelectListItem> AllUsers { get; set; } = new();
        public List<SelectListItem> NotificationTypes { get; set; } = new();
    }
}

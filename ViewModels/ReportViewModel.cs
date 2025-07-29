using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class ReportViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn người dùng cần báo cáo")]
        [Display(Name = "Người dùng cần báo cáo")]
        public int ReportedUserID { get; set; }
        
        public string ReportedUserName { get; set; } = "";
        
        // ID của người đang báo cáo (reporter)
        public int ReporterID { get; set; }
        
        // Tên người đang báo cáo
        public string ReporterName { get; set; } = "";
        
        [Required(ErrorMessage = "Vui lòng chọn loại báo cáo")]
        [Display(Name = "Loại báo cáo")]
        public int TypeID { get; set; }
        
        public IEnumerable<SelectListItem> ReportTypes { get; set; } = new List<SelectListItem>();
        
        // ProjectID là không bắt buộc
        [Display(Name = "Dự án liên quan (không bắt buộc)")]
        public int? ProjectID { get; set; }
        
        public IEnumerable<SelectListItem> Projects { get; set; } = new List<SelectListItem>();
        
        // Danh sách người dùng có thể báo cáo
        public IEnumerable<SelectListItem> UsersList { get; set; } = new List<SelectListItem>();
        
        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết về báo cáo")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Mô tả phải có từ 10 đến 1000 ký tự")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; } = "";
    }
    
    public class ReportListViewModel
    {
        public List<ReportDetailViewModel> Reports { get; set; } = new List<ReportDetailViewModel>();
        public List<SelectListItem> StatusFilter { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TypeFilter { get; set; } = new List<SelectListItem>();
        public int? SelectedStatusId { get; set; }
        public int? SelectedTypeId { get; set; }
        public string SearchQuery { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
    
    public class ReportDetailViewModel
    {
        public int ReportID { get; set; }
        public string ReporterName { get; set; }
        public string ReportedUserName { get; set; }
        public string ProjectTitle { get; set; }
        public int? ProjectID { get; set; }
        public DateTime ReportDate { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public string StatusName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string AdminResponse { get; set; }
    }
} 
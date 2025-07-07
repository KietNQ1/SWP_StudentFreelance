using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class ProjectSubmissionViewModel
    {
        public int SubmissionID { get; set; }
        
        public int ApplicationID { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; }
        
        [Display(Name = "Ngày nộp")]
        public DateTime SubmittedAt { get; set; }
        #nullable enable
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Phản hồi từ doanh nghiệp")]
        public string BusinessFeedback { get; set; }
        
        [Display(Name = "Ngày phản hồi")]
        public DateTime? FeedbackDate { get; set; }
        
        [Display(Name = "File đính kèm")]
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
        
        public List<ProjectSubmissionAttachment> ExistingAttachments { get; set; } = new List<ProjectSubmissionAttachment>();
        
        // Thông tin bổ sung - chỉ dùng để hiển thị, không cần validate
        [Display(Name = "Tên dự án")]
        public string ProjectTitle { get; set; } = "Không xác định";
        
        [Display(Name = "Tên sinh viên")]
        public string StudentName { get; set; } = "Không xác định";
        
        [Display(Name = "Tên doanh nghiệp")]
        public string BusinessName { get; set; } = "Không xác định";
    }
    
    public class ProjectSubmissionListViewModel
    {
        public int ApplicationID { get; set; }
        public string ProjectTitle { get; set; } = "Không xác định";
        public string StudentName { get; set; } = "Không xác định";
        public string ApplicationStatus { get; set; } = "Không xác định";
        public List<ProjectSubmissionViewModel> Submissions { get; set; } = new List<ProjectSubmissionViewModel>();
    }
    
    public class ProjectSubmissionFeedbackViewModel
    {
        public int SubmissionID { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập phản hồi")]
        [Display(Name = "Phản hồi")]
        public string Feedback { get; set; }
        
        [Display(Name = "Chấp nhận kết quả")]
        public bool IsApproved { get; set; }
    }
} 
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
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Phản hồi từ doanh nghiệp")]
        public string BusinessFeedback { get; set; }
        
        [Display(Name = "Ngày phản hồi")]
        public DateTime? FeedbackDate { get; set; }
        
        [Display(Name = "File đính kèm")]
        public List<IFormFile> Attachments { get; set; }
        
        public List<ProjectSubmissionAttachment> ExistingAttachments { get; set; } = new List<ProjectSubmissionAttachment>();
        
        // Thông tin bổ sung
        [Display(Name = "Tên dự án")]
        public string ProjectTitle { get; set; }
        
        [Display(Name = "Tên sinh viên")]
        public string StudentName { get; set; }
        
        [Display(Name = "Tên doanh nghiệp")]
        public string BusinessName { get; set; }
    }
    
    public class ProjectSubmissionListViewModel
    {
        public int ApplicationID { get; set; }
        public string ProjectTitle { get; set; }
        public string StudentName { get; set; }
        public string ApplicationStatus { get; set; }
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
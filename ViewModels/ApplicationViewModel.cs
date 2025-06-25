using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    // ViewModel cho form nộp đơn ứng tuyển
    public class CreateApplicationViewModel
    {
        public int ProjectID { get; set; }
        public string ProjectTitle { get; set; }
        public string BusinessName { get; set; }
        public decimal ProjectBudget { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập thư ứng tuyển")]
        [MinLength(50, ErrorMessage = "Thư ứng tuyển phải có ít nhất 50 ký tự")]
        [Display(Name = "Thư ứng tuyển")]
        public string CoverLetter { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập mức lương đề xuất")]
        [Range(1, 1000000000, ErrorMessage = "Mức lương đề xuất phải lớn hơn 0")]
        [Display(Name = "Mức lương đề xuất (VNĐ)")]
        public decimal Salary { get; set; }
        
        [Display(Name = "Đường dẫn đến portfolio")]
        public string PortfolioLink { get; set; }
        
        [Display(Name = "CV đính kèm")]
        public string ResumeAttachment { get; set; }
    }
    
    // ViewModel cho đơn ứng tuyển của sinh viên (sử dụng trong danh sách đơn ứng tuyển)
    public class StudentApplicationViewModel : ApplicationDetailViewModel
    {
        // Kế thừa tất cả các thuộc tính từ ApplicationDetailViewModel
        // Có thể thêm các thuộc tính đặc biệt cho sinh viên nếu cần
    }
    
    // ViewModel cho danh sách đơn ứng tuyển của sinh viên
    public class StudentApplicationListViewModel
    {
        public List<ApplicationDetailViewModel> Applications { get; set; }
    }
    
    // ViewModel cho danh sách ứng viên của một dự án
    public class ProjectApplicationListViewModel
    {
        public int ProjectID { get; set; }
        public string ProjectTitle { get; set; }
        public List<ApplicationDetailViewModel> Applications { get; set; }
    }
    
    // ViewModel cho xem chi tiết đơn ứng tuyển
    public class ApplicationDetailViewModel
    {
        public int ApplicationID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectTitle { get; set; }
        
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserAvatar { get; set; }
        public decimal? UserAverageRating { get; set; }
        
        public string CoverLetter { get; set; }
        public decimal Salary { get; set; }
        public string Status { get; set; }
        public string TimeAgo { get; set; }
        public List<SkillViewModel> StudentSkills { get; set; }
        
        public string? BusinessNotes { get; set; }
        public string? ResumeAttachment { get; set; }
        public string? PortfolioLink { get; set; }
        
        public DateTime DateApplied { get; set; }
        public DateTime? LastStatusUpdate { get; set; }
        
        public Project Project { get; set; }
    }
    
    // ViewModel cho cập nhật trạng thái đơn ứng tuyển
    public class UpdateApplicationStatusViewModel
    {
        public int ApplicationID { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Status { get; set; }
        
        public List<SelectListItem> StatusOptions { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string? BusinessNotes { get; set; }
    }
    
    // ViewModel cho lên lịch phỏng vấn
    public class ScheduleInterviewViewModel
    {
        public int ApplicationID { get; set; }
        
        [Display(Name = "Tên sinh viên")]
        public string StudentName { get; set; }
        
        [Display(Name = "Tên dự án")]
        public string ProjectTitle { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn thời gian phỏng vấn")]
        [Display(Name = "Thời gian phỏng vấn")]
        public DateTime InterviewDateTime { get; set; }
        
        [Display(Name = "Ghi chú phỏng vấn")]
        public string Notes { get; set; }
    }
    
    // ViewModel đơn giản cho kỹ năng
    public class SkillViewModel
    {
        public int SkillID { get; set; }
        public string SkillName { get; set; }
        public string ProficiencyLevelName { get; set; }
    }
} 
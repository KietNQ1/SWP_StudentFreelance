using Microsoft.AspNetCore.Http;
using StudentFreelance.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.ViewModels
{
    public class UserProfileViewModel
    {
        [Required]
        public int UserID { get; set; }

        public string FullName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public string? CompanyName { get; set; }
        public string? Industry { get; set; }

        // API location IDs
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }

        public string? DetailAddress { get; set; }
        public string? FullAddress { get; set; }

        // Location names for display
        public string? ProvinceName { get; set; }
        public string? DistrictName { get; set; }
        public string? WardName { get; set; }

        public string? AvatarPath { get; set; }
        public IFormFile? AvatarImage { get; set; }

        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<SkillItem> Skills { get; set; } = new();
        public List<OptionItem> AvailableSkills { get; set; } = new();
        public List<OptionItem> AvailableProficiencyLevels { get; set; } = new();
        public List<OptionItem> Provinces { get; set; } = new();
        public List<OptionItem> Districts { get; set; } = new();
        public List<OptionItem> Wards { get; set; } = new();

        public List<RatingViewModel> ReceivedRatings { get; set; } = new();
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // 🆕 Dự án đã hoàn thành (preview + full)
        public List<ProjectHistoryItem> ProjectHistoryPreview { get; set; } = new();
        public List<ProjectHistoryItem> ProjectHistoryAll { get; set; } = new();
        public bool IsCurrentUser { get; set; }

    }

    public class SkillItem
    {
        public int SkillID { get; set; }
        public int ProficiencyLevelID { get; set; }
        public string? SkillName { get; set; }
        public string? ProficiencyLevelName { get; set; }
    }

    public class OptionItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    // 🆕 Lịch sử dự án đã giao/nhận
    public class ProjectHistoryItem
    {
        public int ProjectID { get; set; }
        public string Title { get; set; }
        public string Role { get; set; } // "Business" hoặc "Student"
        public DateTime CompletedDate { get; set; }
        public decimal Budget { get; set; }
        public string TypeName { get; set; }
        public bool IsRemoteWork { get; set; }
        public string CategoryName { get; set; }

        public string? BusinessName { get; set; }             // Tên người tạo (chỉ dùng nếu là Student)
        public DateTime? EndDate { get; set; }                // Thời gian kết thúc dự án
        public string? ShortDescription { get; set; }         // Mô tả ngắn

    }
}

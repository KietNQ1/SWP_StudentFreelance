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
        public int RoleId { get; set; } // Role ID: 3 for Business, 4 for Student

        // VIP subscription properties
        public bool IsVip { get; set; }
        public DateTime? VipExpiryDate { get; set; }

        // 💰 Wallet balance
        public decimal WalletBalance { get; set; }

        // Skill section
        public List<SkillItem> Skills { get; set; } = new();
        public List<OptionItem> AvailableSkills { get; set; } = new();
        public List<OptionItem> AvailableProficiencyLevels { get; set; } = new();

        // Địa chỉ (API)
        public List<OptionItem> Provinces { get; set; } = new();
        public List<OptionItem> Districts { get; set; } = new();
        public List<OptionItem> Wards { get; set; } = new();

        // ⭐ Đánh giá
        public List<RatingViewModel> ReceivedRatings { get; set; } = new();
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }

        // 📜 Phân trang lịch sử dự án
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // 🆕 Dự án đã hoàn thành
        public List<ProjectHistoryItem> ProjectHistoryPreview { get; set; } = new();
        public List<ProjectHistoryItem> ProjectHistoryAll { get; set; } = new();

        // 👤 Xác định người dùng hiện tại
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

        public string? BusinessName { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ShortDescription { get; set; }
    }
}

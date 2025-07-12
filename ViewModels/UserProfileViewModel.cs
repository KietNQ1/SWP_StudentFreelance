using Microsoft.AspNetCore.Http;
using StudentFreelance.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFreelance.ViewModels
{
    public class UserProfileViewModel
    {
        [Required]
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

        // User information
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
        public double? AverageRating { get; set; }  // ⭐ Đánh giá trung bình (1-5 sao)
        public int TotalReviews { get; set; }       // 📊 Tổng số lượt đánh giá

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

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
}

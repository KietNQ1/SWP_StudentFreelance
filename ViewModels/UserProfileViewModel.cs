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

        public int? ProvinceID { get; set; }
        public int? DistrictID { get; set; }
        public int? WardID { get; set; }
        public string? DetailAddress { get; set; }

        public string? AvatarPath { get; set; }
        public IFormFile? AvatarImage { get; set; }

        public List<SkillItem> Skills { get; set; } = new();
        public List<OptionItem> AvailableSkills { get; set; } = new();
        public List<OptionItem> AvailableProficiencyLevels { get; set; } = new();
        public List<OptionItem> Provinces { get; set; } = new();
        public List<OptionItem> Districts { get; set; } = new();
        public List<OptionItem> Wards { get; set; } = new();
    }

    public class SkillItem
    {
        public int SkillID { get; set; }
        public int ProficiencyLevelID { get; set; }
    }

    public class OptionItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}

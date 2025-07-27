using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class InviteStudentViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public string University { get; set; }
        public string Major { get; set; }
        public bool IsVip { get; set; }
        public DateTime? VipExpiryDate { get; set; }
        public List<Skill> MatchedSkills { get; set; }
        public int SkillMatchCount { get; set; }
        public int TotalRequiredSkills { get; set; }
        public double MatchPercentage { get; set; }
    }
} 
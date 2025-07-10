using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class SearchStudentsViewModel
    {
        public List<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<int> SelectedSkillIds { get; set; } = new List<int>();
        public List<OptionItem> Provinces { get; set; } = new List<OptionItem>();
        public Dictionary<int, List<StudentSkill>> StudentSkills { get; set; } = new Dictionary<int, List<StudentSkill>>();
    }
} 
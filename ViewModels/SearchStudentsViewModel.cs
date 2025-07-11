using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class SearchStudentsViewModel
    {
        public List<ApplicationUser> Students { get; set; }
        public List<Skill> Skills { get; set; }
        public List<Category> Categories { get; set; }
        public List<int> SelectedSkillIds { get; set; }

        public SearchStudentsViewModel()
        {
            Students = new List<ApplicationUser>();
            Skills = new List<Skill>();
            Categories = new List<Category>();
            SelectedSkillIds = new List<int>();
        }
    }
} 
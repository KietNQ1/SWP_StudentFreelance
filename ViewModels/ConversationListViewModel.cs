using System.Collections.Generic;
using StudentFreelance.Models;

namespace StudentFreelance.ViewModels
{
    public class ConversationListViewModel
    {
        // Dùng cho dropdown filter Project
        public List<Project> Projects { get; set; }
        public int? SelectedProjectID { get; set; }
        public List<ConversationDto> Items { get; set; }
    }
}

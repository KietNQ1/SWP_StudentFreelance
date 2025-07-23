using StudentFreelance.Models;
using System;
using System.Collections.Generic;

namespace StudentFreelance.ViewModels
{
    public class ProjectDetailViewModel
    {
        public int ProjectID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public DateTime Deadline { get; set; }
        public string CategoryName { get; set; }
        public string TypeName { get; set; }
        public string StatusName { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsRemoteWork { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Business Info
        public int BusinessID { get; set; }
        public string BusinessName { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessAvatarPath { get; set; } // Đường dẫn avatar

        // Kỹ năng yêu cầu
        public List<ProjectSkillItem> RequiredSkills { get; set; } = new();

        // Danh sách file đính kèm
        public List<ProjectAttachment> Attachments { get; set; } = new();
    }

    public class ProjectSkillItem
    {
        public string SkillName { get; set; }
        public string ProficiencyLevelName { get; set; }
    }
}

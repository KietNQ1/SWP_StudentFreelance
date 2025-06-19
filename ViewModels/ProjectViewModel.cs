using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using StudentFreelance.Models;
using StudentFreelance.Models.Enums;

namespace StudentFreelance.ViewModels
{
    public class ProjectViewModel
    {
        public ProjectViewModel()
        {
            // Initialize collections
            Attachments = null;
            SelectedSkills = new List<int>();
            SelectedSkillImportanceLevels = new Dictionary<int, int>();
            Categories = new List<Category>();
            ProjectStatuses = new List<ProjectStatus>();
            ProjectTypes = new List<ProjectType>();
            Skills = new List<Skill>();
            ExistingAttachments = new List<ProjectAttachment>();
            ExistingSkills = new List<ProjectSkillRequired>();
            
            // Set default values
            Title = string.Empty;
            Description = string.Empty;
            BusinessName = string.Empty;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            Deadline = DateTime.Today.AddDays(14);
        }

        public int ProjectID { get; set; }

        [Required(ErrorMessage = "Business ID is required")]
        public int BusinessID { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot be longer than 255 characters")]
        [Display(Name = "Project Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Project Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Budget is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
        [Display(Name = "Budget")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Project Status")]
        public int StatusID { get; set; }

        [Display(Name = "Highlight Project")]
        public bool IsHighlighted { get; set; }

        [Required(ErrorMessage = "Project type is required")]
        [Display(Name = "Project Type")]
        public int TypeID { get; set; }

        [Display(Name = "Address")]
        public int? AddressID { get; set; }

        [Display(Name = "Remote Work")]
        public bool IsRemoteWork { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // For file uploads
        [Display(Name = "Project Attachments")]
        public List<IFormFile>? Attachments { get; set; }

        // For skill selection
        [Display(Name = "Required Skills")]
        public List<int> SelectedSkills { get; set; }

        // For skill importance level selection (SkillID -> ImportanceLevelID)
        [Display(Name = "Skill Importance Levels")]
        public Dictionary<int, int> SelectedSkillImportanceLevels { get; set; }

        // For dropdowns/display
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<ProjectStatus> ProjectStatuses { get; set; }
        public IEnumerable<ProjectType> ProjectTypes { get; set; }
        public IEnumerable<Skill> Skills { get; set; }
        public string BusinessName { get; set; }
        
        // For displaying related data
        public IEnumerable<ProjectAttachment> ExistingAttachments { get; set; }
        public IEnumerable<ProjectSkillRequired> ExistingSkills { get; set; }
        
        // For create/edit distinction
        public bool IsEdit { get; set; }

        // Project status
        public bool IsActive { get; set; } = true;
    }
} 
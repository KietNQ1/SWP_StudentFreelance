using StudentFreelance.Models;
using System;
using System.Collections.Generic;

namespace StudentFreelance.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalBusiness { get; set; }

        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int OngoingProjects { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<ApplicationUser> RecentUsers { get; set; }
        public List<Project> RecentProjects { get; set; }

        public Dictionary<string, int> ProjectStatusCounts { get; set; } = new();
        public Dictionary<string, int> UserRoleCounts { get; set; } = new();
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();

    }
}

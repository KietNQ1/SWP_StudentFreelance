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
        public int TotalModerators { get; set; }

        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int OngoingProjects { get; set; }
        public int CancelledProjects { get; set; }

        public decimal TotalUserWalletBalance { get; set; }
        public int TotalTransactions { get; set; }
        
        // Thêm các thuộc tính thu nhập
        public decimal ProjectFeeIncome { get; set; }
        public decimal VipSubscriptionIncome { get; set; }
        public decimal AdvertisementIncome { get; set; }
        public decimal TotalIncome { get; set; }
        
        // Thêm danh sách giao dịch thu nhập gần đây
        public List<Transaction> RecentIncomeTransactions { get; set; }

        public List<ApplicationUser> RecentUsers { get; set; }
        public List<Project> RecentProjects { get; set; }

        public Dictionary<string, int> ProjectStatusCounts { get; set; } = new();
        public Dictionary<string, int> UserRoleCounts { get; set; } = new();
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();

    }
}

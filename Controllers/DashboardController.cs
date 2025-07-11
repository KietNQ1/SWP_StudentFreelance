using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Models;
using StudentFreelance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();

            var totalStudents = await _context.UserRoles
                .CountAsync(r => r.RoleId == 4); 

            var totalBusiness = await _context.UserRoles
                .CountAsync(r => r.RoleId == 3); 

            var totalProjects = await _context.Projects.CountAsync();
            var completedProjects = await _context.Projects.CountAsync(p => p.StatusID == 3); // Đã hoàn thành
            var ongoingProjects = await _context.Projects.CountAsync(p => p.StatusID == 1);   // Đang tuyển

            var totalRevenue = await _context.Transactions
                .Where(t => t.IsActive && t.TypeID == 1 && t.StatusID == 1)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentProjects = await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var projectStatusCounts = await _context.Projects
                .GroupBy(p => p.Status.StatusName)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var roleCounts = await _context.UserRoles
                .GroupBy(r => r.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var roles = await _context.Roles.ToListAsync();
            var userRoleCounts = roles.ToDictionary(
                r => r.Name,
                r => roleCounts.FirstOrDefault(rc => rc.RoleId == r.Id)?.Count ?? 0
            );

            var now = DateTime.Now;
            var months = Enumerable.Range(0, 12)
                .Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var monthlyRevenue = new Dictionary<string, decimal>();
            foreach (var month in months)
            {
                var label = month.ToString("MM/yyyy");
                var total = await _context.Transactions
                    .Where(t => t.IsActive && t.TypeID == 1 && t.StatusID == 1 &&
                                t.TransactionDate.Year == month.Year &&
                                t.TransactionDate.Month == month.Month)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                monthlyRevenue[label] = total;
            }

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalBusiness = totalBusiness,
                TotalProjects = totalProjects,
                CompletedProjects = completedProjects,
                OngoingProjects = ongoingProjects,
                TotalRevenue = totalRevenue,
                RecentUsers = recentUsers,
                RecentProjects = recentProjects,
                ProjectStatusCounts = projectStatusCounts,
                UserRoleCounts = userRoleCounts,
                MonthlyRevenue = monthlyRevenue
            };

            return View(vm);
        }
    }
}

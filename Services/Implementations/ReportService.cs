using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Interfaces;
using StudentFreelance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Project)
                .Include(r => r.Type)
                .Include(r => r.Status)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Project)
                .Include(r => r.Type)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.ReportID == id && r.IsActive);
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            report.ReportDate = DateTime.Now;
            report.IsActive = true;

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> UpdateReportAsync(Report report)
        {
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return false;
            }

            report.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Report>> GetReportsByUserIdAsync(string userId)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Project)
                .Include(r => r.Type)
                .Include(r => r.Status)
                .Where(r => r.ReporterID.ToString() == userId && r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByProjectIdAsync(int projectId)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Project)
                .Include(r => r.Type)
                .Include(r => r.Status)
                .Where(r => r.ProjectID == projectId && r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByStatusAsync(int statusId)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Project)
                .Include(r => r.Type)
                .Include(r => r.Status)
                .Where(r => r.StatusID == statusId && r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByTypeAsync(int typeId)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Project)
                .Include(r => r.Type)
                .Include(r => r.Status)
                .Where(r => r.TypeID == typeId && r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateReportStatusAsync(int reportId, int statusId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return false;
            }

            report.StatusID = statusId;
            
            // If status is "Resolved" or "Cancelled", update resolved date
            var status = await _context.ReportStatuses.FindAsync(statusId);
            if (status != null && (status.StatusName == "Đã xử lý" || status.StatusName == "Đã hủy"))
            {
                report.ResolvedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
} 
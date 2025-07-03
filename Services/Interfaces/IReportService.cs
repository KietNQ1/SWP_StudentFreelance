using StudentFreelance.Models;

namespace StudentFreelance.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task<Report> GetReportByIdAsync(int id);
        Task<Report> CreateReportAsync(Report report);
        Task<Report> UpdateReportAsync(Report report);
        Task<bool> DeleteReportAsync(int id);
        Task<IEnumerable<Report>> GetReportsByUserIdAsync(string userId);
        Task<IEnumerable<Report>> GetReportsByProjectIdAsync(int projectId);
        Task<IEnumerable<Report>> GetReportsByStatusAsync(int statusId);
        Task<IEnumerable<Report>> GetReportsByTypeAsync(int typeId);
        Task<bool> UpdateReportStatusAsync(int reportId, int statusId);
    }
} 
using StudentFreelance.Models;

namespace StudentFreelance.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeInactive = false, int? userId = null);
        Task<Project> GetProjectByIdAsync(int id, bool includeInactive = false, int? userId = null);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
        Task<bool> ActivateProjectAsync(int id);
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId);
        Task<IEnumerable<Project>> GetProjectsByStatusAsync(int statusId);
        Task<IEnumerable<Project>> GetProjectsByTypeAsync(int typeId);
        Task<bool> UpdateProjectStatusAsync(int projectId, int statusId);
    }
} 
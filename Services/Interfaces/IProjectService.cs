using StudentFreelance.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentFreelance.Services.Interfaces
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
        Task<bool> ConfirmProjectCompletionAsync(int projectId, int applicationId, bool isBusinessConfirmation);
        Task<bool> CompleteProjectAndTransferFundsAsync(int projectId, int applicationId);
        
        // Check if project budget is sufficient for all accepted students
        Task<(bool IsEnough, decimal MissingAmount)> CheckProjectBudgetForAcceptedStudentsAsync(int projectId, int newStudentApplicationId);
        
        // Add funds to project from business wallet
        Task<bool> AddFundsToProjectFromWalletAsync(int projectId, int businessId, decimal amount);

        // Check if user has enough money in wallet
        Task<bool> CheckUserHasEnoughFundsAsync(int userId, decimal amount);

        // Create project with wallet transaction
        Task<(bool Success, Project Project, string ErrorMessage)> CreateProjectWithTransactionAsync(Project project);

        // Update project with wallet transaction (if budget increased)
        Task<(bool Success, Project Project, string ErrorMessage)> UpdateProjectWithTransactionAsync(Project project, decimal originalBudget);
        
        // Complete project by business owner
        Task<(bool Success, string ErrorMessage)> CompleteProjectByBusinessAsync(int projectId, int businessId);
        
        // Cancel project by business owner
        Task<(bool Success, string ErrorMessage)> CancelProjectByBusinessAsync(int projectId, int businessId);
    }
} 
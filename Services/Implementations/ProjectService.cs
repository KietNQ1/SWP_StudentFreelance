using Microsoft.EntityFrameworkCore;
using StudentFreelance.DbContext;
using StudentFreelance.Interfaces;
using StudentFreelance.Models;

namespace StudentFreelance.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeInactive = false, int? userId = null)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Include(p => p.Address)
                .AsQueryable();

            if (includeInactive)
            {
                // Nếu là admin hoặc moderator, hiển thị tất cả dự án
                if (userId.HasValue)
                {
                    // Nếu là người dùng thường, hiển thị dự án active + dự án của họ
                    query = query.Where(p => p.IsActive || p.BusinessID == userId.Value);
                }
                // Nếu không có userId và includeInactive = true, đây là admin/mod, hiển thị tất cả
            }
            else
            {
                // Nếu không bao gồm inactive, chỉ hiển thị dự án đang hoạt động
                query = query.Where(p => p.IsActive);
            }
            
            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int id, bool includeInactive = false, int? userId = null)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Include(p => p.Address)
                .Include(p => p.ProjectSkillsRequired)
                    .ThenInclude(psr => psr.Skill)
                .Include(p => p.ProjectAttachments)
                .Include(p => p.StudentApplications)
                    .ThenInclude(sa => sa.User)
                .Where(p => p.ProjectID == id);

            if (includeInactive)
            {
                if (userId.HasValue)
                {
                    // Nếu là chủ dự án, hiển thị dự án của họ kể cả inactive
                    query = query.Where(p => p.IsActive || p.BusinessID == userId.Value);
                }
                // Nếu là admin/moderator (không có userId), hiển thị tất cả
            }
            else
            {
                // Người dùng thông thường chỉ xem được dự án đang hoạt động
                query = query.Where(p => p.IsActive);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;
            project.IsActive = true;
            
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.ProjectID);
            
            if (existingProject == null)
                return null;
                
            // Update only the properties that should be updated
            existingProject.Title = project.Title;
            existingProject.Description = project.Description;
            existingProject.Budget = project.Budget;
            existingProject.Deadline = project.Deadline;
            existingProject.StatusID = project.StatusID;
            existingProject.IsHighlighted = project.IsHighlighted;
            existingProject.TypeID = project.TypeID;
            existingProject.AddressID = project.AddressID;
            existingProject.IsRemoteWork = project.IsRemoteWork;
            existingProject.CategoryID = project.CategoryID;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existingProject;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
                return false;
                
            // Soft delete - mark as inactive
            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
                return false;
                
            // Activate project
            project.IsActive = true;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId)
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Where(p => p.BusinessID.ToString() == userId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(int statusId)
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Where(p => p.StatusID == statusId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByTypeAsync(int typeId)
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Include(p => p.Business)
                .Where(p => p.TypeID == typeId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateProjectStatusAsync(int projectId, int statusId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            
            if (project == null)
                return false;
                
            project.StatusID = statusId;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 
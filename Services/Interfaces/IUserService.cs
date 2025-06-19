using StudentFreelance.Models;

namespace StudentFreelance.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<ApplicationUser> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UpdateUserStatusAsync(string userId, int statusId);
        Task<IEnumerable<ApplicationUser>> GetUsersByStatusAsync(int statusId);
        Task<IEnumerable<StudentSkill>> GetUserSkillsAsync(string userId);
        Task<bool> AddUserSkillAsync(StudentSkill skill);
        Task<bool> UpdateUserSkillAsync(StudentSkill skill);
        Task<bool> RemoveUserSkillAsync(int skillId, string userId);
    }
} 
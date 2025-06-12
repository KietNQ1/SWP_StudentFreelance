using StudentFreelance.Models;

namespace StudentFreelance.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string id);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UpdateUserStatusAsync(string userId, int statusId);
        Task<IEnumerable<User>> GetUsersByStatusAsync(int statusId);
        Task<IEnumerable<StudentSkill>> GetUserSkillsAsync(string userId);
        Task<bool> AddUserSkillAsync(StudentSkill skill);
        Task<bool> UpdateUserSkillAsync(StudentSkill skill);
        Task<bool> RemoveUserSkillAsync(int skillId, string userId);
    }
} 
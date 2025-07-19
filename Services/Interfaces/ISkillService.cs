using StudentFreelance.Models;
using StudentFreelance.ViewModels;

namespace StudentFreelance.Services.Interfaces
{
    public interface ISkillService
    {
        Task<List<SkillWithCategoryViewModel>> GetActiveSkillsWithCategoriesAsync();
        Task<Skill?> GetByIdAsync(int id);
        Task CreateAsync(Skill skill);
        Task UpdateAsync(Skill skill);
        Task SoftDeleteAsync(int id);
        Task UnhideAsync(int id);
    }

}

namespace StudentFreelance.ViewModels
{
    public class SkillWithCategoryViewModel
    {
        public int SkillID { get; set; }
        public string SkillName { get; set; }
        public string SubCategory { get; set; }
        public string? ParentCategory { get; set; }
    }

}

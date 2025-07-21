namespace StudentFreelance.Models
{
    public class CVInputModel
    {
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string Industry { get; set; }
        public string University { get; set; }
        public string GPA { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }
        public string CareerGoal { get; set; }
        public string SoftSkills { get; set; }

        public string Template { get; set; }
        public string ProjectsText { get; set; }

        public List<string> ProjectsRaw { get; set; } = new List<string>();
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public IFormFile AvatarFile { get; set; } // Dùng khi upload từ form
        public string AvatarPath { get; set; }    // Đường dẫn ảnh sau khi lưu


    }
}

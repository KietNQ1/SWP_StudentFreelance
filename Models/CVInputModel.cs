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

        public string Template { get; set; }
        public List<string> ProjectsRaw { get; set; } = new List<string>();

    }
}

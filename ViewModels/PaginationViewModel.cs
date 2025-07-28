using System.Collections.Generic;

namespace StudentFreelance.ViewModels
{
    public class PaginationViewModel
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();
        public string ItemName { get; set; } = "mục"; // Default item name
    }
} 
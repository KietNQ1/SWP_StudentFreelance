using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StudentFreelance.Models;
using AspNetCoreGeneratedDocument;

namespace StudentFreelance.Controllers
{
    public class CVController : Controller
    {
        [HttpGet]
        public IActionResult ChooseTemplate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult InputInfo(string template)
        {
            
            return View(new CVInputModel { Template = template });
        }

        private string GeneratePrompt(CVInputModel model)
        {
            return $"""
    Viết một bản CV bằng tiếng Việt cho sinh viên với thông tin đầu vào bên dưới. 
    Yêu cầu: mở rộng nội dung, bổ sung kỹ năng liên quan (ví dụ: nếu có C# thì gợi ý thêm .NET, Git...), diễn giải cho chuyên nghiệp hơn.

    Đầu vào:
    Họ tên: {model.FullName}
    Ngành học: {model.Industry}
    Vị trí ứng tuyển: {model.JobTitle}
    Trường đại học: {model.University}
    GPA: {model.GPA}
    Kỹ năng: {model.Skills}
    Kinh nghiệm: {model.Experience}
    Mục tiêu nghề nghiệp: {model.CareerGoal}

    Trả về đúng định dạng sau (không thêm giải thích):

    FullName: ...
    Industry: ...
    JobTitle: ...
    University: ...
    GPA: ...
    CareerGoal: ...
    Skills: ...
    Experience: ...
    Projects:
    - Tên: ...
      Mô tả: ...
      Công nghệ: ...
    - Tên: ...
      Mô tả: ...
      Công nghệ: ...
    """;
        }

        private async Task<string> CallGeminiAPI(string prompt)
        {
            string apiKey = "AIzaSyCoiQgI47KSy0wALt9EKaklCSkM-0lYRGU"; 
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            using var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Lấy text kết quả từ response JSON
            var json = JObject.Parse(responseContent);
            var text = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            return text ?? "Không có phản hồi từ Gemini.";
        }

        private CVInputModel ParseGeminiResponse(string response)
        {
            var model = new CVInputModel();
            var lines = response.Split('\n');

            var currentProject = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("FullName:"))
                    model.FullName = line.Replace("FullName:", "").Trim();
                else if (line.StartsWith("Industry:"))
                    model.Industry = line.Replace("Industry:", "").Trim();
                else if (line.StartsWith("JobTitle:"))
                    model.JobTitle = line.Replace("JobTitle:", "").Trim();
                else if (line.StartsWith("University:"))
                    model.University = line.Replace("University:", "").Trim();
                else if (line.StartsWith("GPA:"))
                    model.GPA = line.Replace("GPA:", "").Trim();
                else if (line.StartsWith("CareerGoal:"))
                    model.CareerGoal = line.Replace("CareerGoal:", "").Trim();
                else if (line.StartsWith("Skills:"))
                    model.Skills = line.Replace("Skills:", "").Trim();
                else if (line.StartsWith("Experience:"))
                    model.Experience = line.Replace("Experience:", "").Trim();
                else if (line.StartsWith("- Tên:") || line.Trim().StartsWith("Mô tả:") || line.Trim().StartsWith("Công nghệ:"))
                    currentProject.Add(line.Trim());
            }

            model.ProjectsRaw = currentProject; 
            return model;
        }
        [HttpPost]
        public async Task<IActionResult> Preview(CVInputModel model)
        {
            string prompt = GeneratePrompt(model);
            string response = await CallGeminiAPI(prompt);

            var parsedModel = ParseGeminiResponse(response);
            parsedModel.Template = model.Template; 

            return View("Classic", parsedModel);
        }


    }
}

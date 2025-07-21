using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StudentFreelance.Models;
using AspNetCoreGeneratedDocument;
using System.Text.RegularExpressions;
using DinkToPdf.Contracts;
using DinkToPdf;

namespace StudentFreelance.Controllers
{
    public class CVController : Controller
    {
        private readonly IConverter _converter;

        public CVController(IConverter converter)
        {
            _converter = converter;
        }
        [HttpGet]
        public IActionResult inputInfo()
        {
            return View();
        }
        private string GeneratePrompt(CVInputModel model)
        {
            var projectsInput = string.Join("\n", model.ProjectsRaw ?? new List<string>());
            return $"""
Bạn là chuyên gia nhân sự có kinh nghiệm viết CV cho sinh viên hoặc người mới ra trường ở nhiều lĩnh vực như IT, Marketing, Kế toán, Du lịch, Y dược...

Yêu cầu:
- Diễn đạt lại nội dung chuyên nghiệp hơn.
- Nếu thiếu kỹ năng quan trọng trong ngành, hãy bổ sung.
- Nếu phần kinh nghiệm chưa đủ, gợi ý thêm dự án hoặc hoạt động thực tế phù hợp.
- Nếu có ít dự án, hãy tự tạo thêm 1 dự án logic dựa trên ngành và vị trí.
- Điều chỉnh mục tiêu nghề nghiệp sao cho hợp lý.
- Bổ sung thêm kỹ năng mềm (giao tiếp, làm việc nhóm, ngoại ngữ...) nếu chưa có.

Thông tin đầu vào:
- Họ tên: {model.FullName}
- Ngành học: {model.Industry}
- Vị trí ứng tuyển: {model.JobTitle}
- Trường: {model.University}
- GPA: {model.GPA}
- Kỹ năng: {model.Skills}
- Kinh nghiệm: {model.Experience}
- Mục tiêu nghề nghiệp: {model.CareerGoal}
- Dự án đã làm:
{projectsInput}

Trả về đúng định dạng sau (không thêm giải thích ngoài nội dung):

FullName: ...
Industry: ...
JobTitle: ...
University: ...
GPA: ...
CareerGoal: ...
Skills: ...
SoftSkills: ...
Experience: ...
Projects:
- Tên: ...
  vai trò: ...
  Mô tả: ...
  Công nghệ / Công cụ: ...

- Tên: ...
  vai trò: ...
  Mô tả: ...
  Công nghệ / Công cụ: ...
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

            // 1. Các trường một dòng
            string GetSingleLine(string label)
            {
                return Regex.Match(response, $@"(?im)^{label}:\s*(.+)$").Groups[1].Value.Trim();
            }

            model.FullName = GetSingleLine("FullName");
            model.Industry = GetSingleLine("Industry");
            model.JobTitle = GetSingleLine("JobTitle");
            model.University = GetSingleLine("University");
            model.GPA = GetSingleLine("GPA");
            model.CareerGoal = GetMultilineBlock(response, "CareerGoal", new[] { "Skills", "Experience", "Projects" });
            model.Skills = GetMultilineBlock(response, "Skills", new[] { "Experience", "Projects" });
            model.SoftSkills = GetMultilineBlock(response, "SoftSkills", new[] { "Experience", "Projects" });
            model.Experience = GetMultilineBlock(response, "Experience", new[] { "Projects" });

            // 2. Tách Projects
            var projectLines = new List<string>();
            var lines = Regex.Split(response, @"\r?\n");
            var pattern = new Regex(
                @"^\s*(-\s*Tên:.*|" +
                @"\s*vai trò:.*|" +
                @"\s*Mô tả:.*|" +
                @"\s*Công nghệ\s*(/|:).*)$",
                RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                if (pattern.IsMatch(line))
                    projectLines.Add(line.Trim());
            }

            model.ProjectsRaw = projectLines;
            return model;
        }

        // 👇 Hàm phụ: bắt một khối nội dung từ label cho đến label tiếp theo
        private string GetMultilineBlock(string response, string startLabel, string[] endLabels)
        {
            string pattern = $@"(?ims)^{startLabel}:\s*(.*?)(^\s*({string.Join("|", endLabels)})\s*:)";
            var match = Regex.Match(response, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Nếu không có label sau, lấy đến cuối
            var fallback = Regex.Match(response, $@"(?ims)^{startLabel}:\s*(.*)", RegexOptions.Multiline);
            return fallback.Success ? fallback.Groups[1].Value.Trim() : "";
        }


        [HttpPost]
        public async Task<IActionResult> Preview(CVInputModel model)
        {
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "submissions");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.AvatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                // Lưu đúng đường dẫn để hiển thị
                model.AvatarPath = fileName;
            }

            // Gọi Gemini API và parse
            string prompt = GeneratePrompt(model);
            string response = await CallGeminiAPI(prompt);
            var parsedModel = ParseGeminiResponse(response);

            // Truyền dữ liệu cho view
            parsedModel.Template = model.Template;
            parsedModel.AvatarPath = model.AvatarPath;
            ViewBag.RawGemini = response;

            return View("Classic", parsedModel);
        }


        [HttpPost]
        public IActionResult EditPreview(CVInputModel model, string ProjectsText)
        {
            model.ProjectsRaw = ProjectsText?
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()).ToList();

            // Hiển thị lại view "Classic" nhưng với dữ liệu đã chỉnh sửa
            return View("Classic", model);
        }
        [HttpPost]
        public IActionResult ExportToPDF(CVInputModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.ProjectsText))
            {
                model.ProjectsRaw = model.ProjectsText
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            string css = GetCssContent(); 

      
            string htmlView = RenderViewToString(this, "ClassicForPdf", model);

           
            string htmlWithCss = $"<style>{css}</style>{htmlView}";

            
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4
        },
                Objects = {
            new ObjectSettings()
            {
                HtmlContent = htmlWithCss,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdfBytes = _converter.Convert(doc);
            return File(pdfBytes, "application/pdf", "CV.pdf");
        }



        private string RenderViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using var writer = new StringWriter();
            var viewResult = Microsoft.AspNetCore.Mvc.ViewEngines.ViewEngineResult.Found(viewName,
                controller.HttpContext.RequestServices
                    .GetRequiredService<Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine>()
                    .FindView(controller.ControllerContext, viewName, false).View);

            var viewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
            );

            viewResult.View.RenderAsync(viewContext).Wait();
            return writer.GetStringBuilder().ToString();
        }
        private string GetCssContent()
        {
            // Đường dẫn vật lý tới file wwwroot/css/cv1.css
            var wwwRoot = Directory.GetCurrentDirectory();
            var cssPath = Path.Combine(wwwRoot, "wwwroot", "css", "cv1.css");

            if (System.IO.File.Exists(cssPath))
            {
                return System.IO.File.ReadAllText(cssPath);
            }

            return ""; // Tránh lỗi nếu không tìm thấy file
        }



    }
}

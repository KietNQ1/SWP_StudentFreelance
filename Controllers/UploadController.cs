using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StudentFreelance.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int conversationId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            // 📁 Tạo thư mục theo conversationID
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", $"convo_{conversationId}");
            Directory.CreateDirectory(uploadsFolder); // Tự tạo nếu chưa có

            // 🧠 Tạo tên file ngẫu nhiên, giữ phần mở rộng gốc
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream); // Ghi file lên ổ đĩa
            }

            // 🔗 Trả đường dẫn public để chat client hiển thị
            string fileUrl = $"/uploads/convo_{conversationId}/{fileName}";
            return Ok(new { fileUrl });
        }
    }
}

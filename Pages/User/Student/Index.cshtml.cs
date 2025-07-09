using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace api.Pages.User.Student
{
    public class IndexModel : PageModel
    {
        public List<ExamInfo> Exams { get; set; } = new();
        public string StudentName { get; set; }
        public string StudentAvatar { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy thông tin Student từ TempData hoặc Session
            var studentId = TempData["StudentId"]?.ToString();
            var studentName = TempData["StudentName"]?.ToString();
            var studentAvatar = TempData["StudentAvatar"]?.ToString();
            var classIdStr = TempData["StudentClass"]?.ToString();

            // Nếu dùng Session, thay TempData bằng HttpContext.Session.GetString(...)

            if (string.IsNullOrEmpty(classIdStr))
            {
                // Nếu chưa có thông tin, chuyển về login
                return RedirectToPage("/User/Student/Login");
            }

            StudentName = studentName ?? "Student";
            StudentAvatar = studentAvatar ?? "";

            int classId = int.Parse(classIdStr);

            // Lấy danh sách Exam theo IdClass
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/class/{classId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Exams = JsonConvert.DeserializeObject<List<ExamInfo>>(json) ?? new List<ExamInfo>();
            }

            // Giữ lại TempData cho request tiếp theo
            TempData.Keep();

            return Page();
        }

        public class ExamInfo
        {
            public int ExamId { get; set; }
            public string Title { get; set; }
            public DateTime ExamDate { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace api.Pages.User.Students
{
    public class ScheduleCalendarModel : PageModel
    {
        public string StudentName { get; set; }
        public string StudentAvatar { get; set; }
        public string StudentClassId { get; set; }

        public List<ExamInfo> Exams { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = HttpContext.Session.GetString("StudentId");
            var studentName = HttpContext.Session.GetString("StudentName");
            var studentAvatar = HttpContext.Session.GetString("StudentAvatar");
            var classIdStr = HttpContext.Session.GetString("StudentClass");

            if (string.IsNullOrEmpty(classIdStr))
            {
                return RedirectToPage("/User/Students/Login");
            }

            StudentName = studentName ?? "Student";
            StudentAvatar = studentAvatar ?? "";
            StudentClassId = classIdStr;

            if (!int.TryParse(classIdStr, out var classId))
            {
                ModelState.AddModelError(string.Empty, "Lỗi định dạng lớp học.");
                return Page();
            }

            using var httpClient = new HttpClient();

            try
            {
                var examRes = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/class/{classId}");
                if (examRes.IsSuccessStatusCode)
                {
                    var json = await examRes.Content.ReadAsStringAsync();
                    Exams = JsonConvert.DeserializeObject<List<ExamInfo>>(json) ?? new List<ExamInfo>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi khi lấy dữ liệu Exam: " + ex.Message);
            }

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

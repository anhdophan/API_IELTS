using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using api.Models;
using System.Linq;

namespace api.Pages.User.Students
{
    public class DoExamModel : PageModel
    {
        [BindProperty]
        public int ExamId { get; set; }
        [BindProperty]
        public List<string> Answers { get; set; } = new();

        public Exam Exam { get; set; }
        public List<Question> Questions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int examId)
        {
            ExamId = examId;
            using var httpClient = new HttpClient();
            var examRes = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{examId}");
            if (!examRes.IsSuccessStatusCode) return NotFound();

            var examJson = await examRes.Content.ReadAsStringAsync();
            Exam = JsonConvert.DeserializeObject<Exam>(examJson);

            // Lấy danh sách câu hỏi
            var qRes = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{examId}/questions");
            if (!qRes.IsSuccessStatusCode) return NotFound();

            var qJson = await qRes.Content.ReadAsStringAsync();
            Questions = JsonConvert.DeserializeObject<List<Question>>(qJson);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Lấy StudentId từ Session hoặc TempData
            var studentIdStr = HttpContext.Session.GetString("StudentId") ?? TempData["StudentId"]?.ToString();
            if (string.IsNullOrEmpty(studentIdStr))
                return RedirectToPage("/User/Students/Login");

            int studentId = int.Parse(studentIdStr);

            using var httpClient = new HttpClient();

            // Gửi kết quả lên API
            var submitData = new
            {
                StudentId = studentId,
                ExamId = ExamId,
                Answers = Answers,
                DurationSeconds = 0 // Có thể tính thời gian thực tế nếu muốn
            };
            var content = new StringContent(JsonConvert.SerializeObject(submitData), System.Text.Encoding.UTF8, "application/json");
            var res = await httpClient.PostAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{ExamId}/submit", content);

            if (res.IsSuccessStatusCode)
            {
                return RedirectToPage("ExamPage", new { examId = ExamId });
            }
            else
            {
                ModelState.AddModelError("", "Nộp bài thất bại. Vui lòng thử lại.");
                await OnGetAsync(ExamId);
                return Page();
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class CreateQuestionModel : PageModel
    {
        [BindProperty]
        public Question Question { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Gán CreatedById từ Session (ví dụ: session "TeacherId")
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToPage("/User/Teachers/Login");
            var createdById = HttpContext.Session.GetString("TeacherId");
           
            Question.CreatedById = createdById;
            

            // Lấy chuỗi đáp án từ form
            var choicesInputRaw = Request.Form["ChoicesInput"].ToString();

            if (Question.IsMultipleChoice)
            {
                // Parse danh sách đáp án từ textarea
                if (!string.IsNullOrEmpty(choicesInputRaw))
                {
                    Question.Choices = choicesInputRaw
                        .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
                }

                // Parse chỉ số đáp án đúng
                var correctIndexStr = Request.Form["CorrectAnswerIndex"];
                if (int.TryParse(correctIndexStr, out int correctIndex))
                {
                    Question.CorrectAnswerIndex = correctIndex;
                }
            }
            else
            {
                // Nếu là tự luận, lấy đáp án từ trường input
                Question.CorrectInputAnswer = Request.Form["CorrectInputAnswer"];
                Question.Choices = new List<string>(); // Rõ ràng là không có choices
                Question.CorrectAnswerIndex = null;
            }

            // Gửi POST đến API
            using var httpClient = new HttpClient();
            var jsonContent = JsonConvert.SerializeObject(Question);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api-ielts-cgn8.onrender.com/api/Question", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Questions"); // hoặc nơi bạn muốn chuyển tới
            }

            ModelState.AddModelError(string.Empty, "Tạo câu hỏi thất bại. Vui lòng thử lại.");
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class CreateQuestionModel : PageModel
    {
        [BindProperty]
        public Question Question { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToPage("/User/Teachers/Login");

            Question.CreatedById = teacherId;

           if (Question.IsMultipleChoice)
{
    var choicesInputRaw = Request.Form["ChoicesInput"].ToString();
    if (!string.IsNullOrWhiteSpace(choicesInputRaw))
    {
        Question.Choices = choicesInputRaw
            .Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    // Tự luận không có field này
    Question.CorrectInputAnswer = null;
}
else
{
    Question.Choices = new List<string>();
    Question.CorrectAnswerIndex = null;

    // Gán đúng giá trị từ form
    var inputAnswer = Request.Form["Question.CorrectInputAnswer"];
    Question.CorrectInputAnswer = !string.IsNullOrWhiteSpace(inputAnswer) ? inputAnswer.ToString().Trim() : null;
}

            using var httpClient = new HttpClient();
            var jsonContent = JsonConvert.SerializeObject(Question);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api-ielts-cgn8.onrender.com/api/Question", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Questions");

            ModelState.AddModelError(string.Empty, "Tạo câu hỏi thất bại. Vui lòng thử lại.");
            return Page();
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace api.Pages.Admin.Questions
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Question Question { get; set; }

        [BindProperty]
        public string ChoicesInput { get; set; } // one per line

        public void OnGet()
        {
            Question = new Question();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Parse Choices
            if (Question.IsMultipleChoice && !string.IsNullOrWhiteSpace(ChoicesInput))
            {
                Question.Choices = new List<string>(ChoicesInput.Split('\n', '\r'));
                Question.Choices.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            }
            else
            {
                Question.Choices = null;
                Question.CorrectAnswerIndex = null;
            }

            // Gán CreatedById (ví dụ: Admin là "00")
            if (string.IsNullOrEmpty(Question.CreatedById))
                Question.CreatedById = "00";

            if (!ModelState.IsValid)
                return Page();

            using var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(Question);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api-ielts-cgn8.onrender.com/api/Question", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Không thể tạo câu hỏi. Hãy kiểm tra lại dữ liệu.");
            return Page();
        }
    }
}
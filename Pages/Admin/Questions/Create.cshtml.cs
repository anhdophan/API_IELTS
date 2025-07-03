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
        public string LevelsInput { get; set; } // comma separated

        public void OnGet()
        {
            Question = new Question { Choices = new List<string>() };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Parse Levels
            if (!string.IsNullOrWhiteSpace(LevelsInput))
            {
                Question.Levels = new List<double>();
                foreach (var s in LevelsInput.Split(','))
                {
                    if (double.TryParse(s.Trim(), out var lvl))
                        Question.Levels.Add(lvl);
                }
            }

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
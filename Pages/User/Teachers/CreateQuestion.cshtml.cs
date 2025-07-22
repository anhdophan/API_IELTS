using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using api.Models;

namespace api.Pages.User.Teachers.Questions
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Question Question { get; set; } = new();

        public IActionResult OnGet()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
            {
                return RedirectToPage("/Login");
            }

            Question.CreatedById = int.Parse(teacherId);

            using var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(Question);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api-ielts-cgn8.onrender.com/api/Question", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/User/Teachers/QuestionList");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo câu hỏi.");
                return Page();
            }
        }
    }
}

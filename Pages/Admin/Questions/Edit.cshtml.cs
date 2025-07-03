using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace api.Pages.Admin.Questions
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Question Question { get; set; }

        [BindProperty]
        public string ChoicesInput { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Question/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var json = await response.Content.ReadAsStringAsync();
            Question = JsonConvert.DeserializeObject<Question>(json);

            // Convert Choices to string for editing
            ChoicesInput = Question.Choices != null ? string.Join("\n", Question.Choices) : "";

            return Page();
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

            if (string.IsNullOrEmpty(Question.CreatedById))
                Question.CreatedById = "00";

            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Question);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"https://api-ielts-cgn8.onrender.com/api/Question/{Question.QuestionId}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Không thể cập nhật câu hỏi.");
            return Page();
        }
    }
}
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
        public string ChoicesInput { get; set; } = string.Empty; // nhận từ hidden input

        public string DebugMessage { get; set; }

        public void OnGet()
        {
            Question = new Question();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Xử lý ChoicesInput
            if (Question.IsMultipleChoice)
            {
                if (!string.IsNullOrWhiteSpace(ChoicesInput))
                {
                    Question.Choices = new List<string>(ChoicesInput.Split('\n'));
                    Question.Choices.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                }
                else
                {
                    Question.Choices = new List<string>();
                }
            }
            else
            {
                Question.Choices = new List<string>();
                Question.CorrectAnswerIndex = null;

                // XÓA lỗi ràng buộc ChoicesInput nếu không phải MultipleChoice
                ModelState.Remove(nameof(ChoicesInput));
            }

            if (string.IsNullOrEmpty(Question.CreatedById))
                Question.CreatedById = "00";

            if (!ModelState.IsValid)
            {
                DebugMessage = "ModelState is invalid.";
                return Page();
            }

            using var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(Question);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("https://api-ielts-cgn8.onrender.com/api/Question", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToPage("Index");

                var errorContent = await response.Content.ReadAsStringAsync();
                DebugMessage = $"API Error: {response.StatusCode} - {errorContent}";
                ModelState.AddModelError(string.Empty, "Không thể tạo câu hỏi. Hãy kiểm tra lại dữ liệu.");
            }
            catch (System.Exception ex)
            {
                DebugMessage = $"Exception: {ex.Message}";
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi gửi dữ liệu.");
            }

            return Page();
        }


    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace api.Pages.Admin.Exams
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public Exam Exam { get; set; }

        [BindProperty]
        public List<Question> AllQuestions { get; set; } = new();

        [BindProperty]
        public List<int> SelectedQuestionIds { get; set; } = new();

        [BindProperty]
        public List<double> SelectedScores { get; set; } = new();

        [BindProperty]
        public double FilterLevel { get; set; }

        public string DebugMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadQuestionsAsync();
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var json = await response.Content.ReadAsStringAsync();
            Exam = JsonConvert.DeserializeObject<Exam>(json);

            // Gán SelectedQuestionIds và SelectedScores từ Exam
            if (Exam.Questions != null)
            {
                SelectedQuestionIds = Exam.Questions.Select(q => q.QuestionId).ToList();
                SelectedScores = Exam.Questions.Select(q => q.Score).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostFilterAsync(int id)
        {
            await LoadQuestionsAsync(FilterLevel);

            // Lấy lại Exam để giữ các trường khác
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Exam = JsonConvert.DeserializeObject<Exam>(json);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await LoadQuestionsAsync();

            // Build Exam.Questions từ selected ids và scores
            Exam.Questions = new List<ExamQuestion>();
            for (int i = 0; i < SelectedQuestionIds.Count; i++)
            {
                if (SelectedScores.Count > i && SelectedScores[i] > 0)
                {
                    Exam.Questions.Add(new ExamQuestion
                    {
                        QuestionId = SelectedQuestionIds[i],
                        Score = SelectedScores[i]
                    });
                }
            }

            if (string.IsNullOrEmpty(Exam.CreatedById))
                Exam.CreatedById = "00";

            if (Exam.Questions.Count == 0)
            {
                DebugMessage = "Please select at least one question and set its score.";
                return Page();
            }

            using var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(Exam);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var errorContent = await response.Content.ReadAsStringAsync();
            DebugMessage = $"API Error: {response.StatusCode} - {errorContent}";
            return Page();
        }

        private async Task LoadQuestionsAsync(double? level = null)
        {
            using var httpClient = new HttpClient();
            string url = "https://api-ielts-cgn8.onrender.com/api/Question/all";
            if (level.HasValue)
                url = $"https://api-ielts-cgn8.onrender.com/api/Question/level/{level.Value}";

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                AllQuestions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new List<Question>();
            }
        }
    }
}
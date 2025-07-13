using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;

namespace api.Pages.Admin.Exams
{
    public class CreateModel : PageModel
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

        public List<Class> AllClasses { get; set; } = new();

        public string DebugMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadQuestionsAsync();
            await LoadClassesAsync();
        }

        public async Task<IActionResult> OnPostFilterAsync()
        {
            await LoadQuestionsAsync(FilterLevel);
            await LoadClassesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadQuestionsAsync();
            await LoadClassesAsync();

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

            // ✅ Giữ nguyên local time (không chuyển sang UTC)
            if (Exam.StartTime.Kind == DateTimeKind.Unspecified)
                Exam.StartTime = DateTime.SpecifyKind(Exam.StartTime, DateTimeKind.Local);
            if (Exam.EndTime.Kind == DateTimeKind.Unspecified)
                Exam.EndTime = DateTime.SpecifyKind(Exam.EndTime, DateTimeKind.Local);

            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local // ⛳ KHÔNG chuyển sang UTC
            };

            var json = JsonConvert.SerializeObject(Exam, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await new HttpClient().PostAsync("https://api-ielts-cgn8.onrender.com/api/Exam", content);

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

        private async Task LoadClassesAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                AllClasses = JsonConvert.DeserializeObject<List<Class>>(json) ?? new List<Class>();
            }
        }
    }
}

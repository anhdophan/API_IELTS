using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace api.Pages.User.Teachers
{
    public class CreateExamModel : PageModel
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

            string teacherId = HttpContext.Session.GetString("TeacherId");
            Exam.CreatedById = teacherId ?? "1";

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

            if (Exam.Questions.Count == 0)
            {
                DebugMessage = "Vui lòng chọn ít nhất một câu hỏi và nhập điểm hợp lệ.";
                return Page();
            }

            if (Exam.StartTime.Kind == DateTimeKind.Unspecified)
                Exam.StartTime = DateTime.SpecifyKind(Exam.StartTime, DateTimeKind.Local);
            if (Exam.EndTime.Kind == DateTimeKind.Unspecified)
                Exam.EndTime = DateTime.SpecifyKind(Exam.EndTime, DateTimeKind.Local);

            var json = JsonConvert.SerializeObject(Exam, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await new HttpClient().PostAsync("https://api-ielts-cgn8.onrender.com/api/Exam", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Exams");

            var errorContent = await response.Content.ReadAsStringAsync();
            DebugMessage = $"Lỗi từ API: {response.StatusCode} - {errorContent}";
            return Page();
        }

        private async Task LoadQuestionsAsync(double? level = null)
        {
            string url = "https://api-ielts-cgn8.onrender.com/api/Question/all";
            if (level.HasValue)
                url = $"https://api-ielts-cgn8.onrender.com/api/Question/level/{level.Value}";

            var response = await new HttpClient().GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                AllQuestions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new();
            }
        }

        private async Task LoadClassesAsync()
        {
            string teacherId = HttpContext.Session.GetString("TeacherId");
            var response = await new HttpClient().GetAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}/classes");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                AllClasses = JsonConvert.DeserializeObject<List<Class>>(json) ?? new();
            }
        }
    }
}

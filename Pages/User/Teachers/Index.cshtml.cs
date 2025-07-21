using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class IndexModel : PageModel
    {
        public Teacher Teacher { get; set; }
        public List<Class> Classes { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
        public List<Question> Questions { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            using var httpClient = new HttpClient();

            var teacherRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}");
            Teacher = JsonConvert.DeserializeObject<Teacher>(teacherRes);

            var classesRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Class/teacher/{teacherId}");
            Classes = JsonConvert.DeserializeObject<List<Class>>(classesRes);

            var examsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/teacher/{teacherId}");
            Exams = JsonConvert.DeserializeObject<List<Exam>>(examsRes);

            var questionsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Question/teacher/{teacherId}");
            Questions = JsonConvert.DeserializeObject<List<Question>>(questionsRes);
        }
    }
}
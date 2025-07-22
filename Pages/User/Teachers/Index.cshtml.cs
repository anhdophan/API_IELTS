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
        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            using var httpClient = new HttpClient();

            var teacherRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}");
            Teacher = JsonConvert.DeserializeObject<Teacher>(teacherRes);

            // Lấy danh sách lớp
            try
            {
                var classRes = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
                List<Class> classes = new();
                try
                {
                    classes = JsonConvert.DeserializeObject<List<Class>>(classRes);
                }
                catch
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Class>>(classRes);
                    classes = dict?.Values.ToList() ?? new List<Class>();
                }
                Classes = classes;
            }
            catch
            {
                Classes = new List<Class>();
            }

            // Lấy danh sách bài thi
            try
            {
                var examsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/teacher/{teacherId}");
                Exams = JsonConvert.DeserializeObject<List<Exam>>(examsRes);
            }
            catch
            {
                Exams = new List<Exam>();
            }

            // Lấy danh sách câu hỏi
            try
            {
                var questionsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Question/teacher/{teacherId}");
                Questions = JsonConvert.DeserializeObject<List<Question>>(questionsRes);
            }
            catch
            {
                Questions = new List<Question>();
            }

            // Lấy danh sách khóa học
            try
            {
                var courseRes = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Course/all");
                List<Course> courses = new();
                try
                {
                    courses = JsonConvert.DeserializeObject<List<Course>>(courseRes);
                }
                catch
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Course>>(courseRes);
                    courses = dict?.Values.ToList() ?? new List<Course>();
                }
                Courses = courses;
            }
            catch
            {
                Courses = new List<Course>();
            }
        }
    }
}
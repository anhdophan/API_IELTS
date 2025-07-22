using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Pages.User.Teachers
{
    public class IndexModel : PageModel
    {
        public Teacher Teacher { get; set; }
        public List<Class> Classes { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<TeachingScheduleInfo> TeachingSchedules { get; set; } = new();
        public List<ExamInfo> UpcomingExams { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToPage("/User/Teachers/Login");

            using var httpClient = new HttpClient();

            // Lấy thông tin Teacher
            var teacherRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}");
            Teacher = JsonConvert.DeserializeObject<Teacher>(teacherRes);

            // Lấy lịch học
            try
            {
                var scheduleRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}/schedule");
                TeachingSchedules = JsonConvert.DeserializeObject<List<TeachingScheduleInfo>>(scheduleRes) ?? new List<TeachingScheduleInfo>();
            }
            catch
            {
                TeachingSchedules = new List<TeachingScheduleInfo>();
            }

            // Lấy lịch thi
            try
            {
                var examRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}/examschedule");
                UpcomingExams = JsonConvert.DeserializeObject<List<ExamInfo>>(examRes) ?? new List<ExamInfo>();
            }
            catch
            {
                UpcomingExams = new List<ExamInfo>();
            }

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

            return Page();
        }
    }

    public class TeachingScheduleInfo
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Room { get; set; }
    }

    public class ExamInfo
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public DateTime ExamDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ClassId { get; set; }
        public string Room { get; set; }
    }
}
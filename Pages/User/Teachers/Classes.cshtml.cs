using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class ClassesModel : PageModel
    {
        public List<Class> Classes { get; set; } = new();
        public Dictionary<int, string> CourseNames { get; set; } = new();
        public Dictionary<int, List<Student>> StudentsInClass { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            using var httpClient = new HttpClient();

            // Lấy danh sách lớp theo giáo viên
            var res = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Class/filter?teacherId={teacherId}");
            List<Class> classes = new();
            try
            {
                classes = JsonConvert.DeserializeObject<List<Class>>(res);
            }
            catch
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Class>>(res);
                classes = dict?.Values.ToList() ?? new List<Class>();
            }
            Classes = classes;

            // Lấy danh sách khóa học
            var courseRes = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Course/all");
            var courses = JsonConvert.DeserializeObject<List<Course>>(courseRes) ?? new List<Course>();
            CourseNames = courses.ToDictionary(c => c.CourseId, c => c.Name);

            // Lấy danh sách học sinh theo từng lớp
            StudentsInClass = new Dictionary<int, List<Student>>();
            foreach (var cls in Classes)
            {
                try
                {
                    var studentRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{cls.ClassId}/students");
                    var students = JsonConvert.DeserializeObject<List<Student>>(studentRes) ?? new List<Student>();
                    StudentsInClass[cls.ClassId] = students;
                }
                catch
                {
                    StudentsInClass[cls.ClassId] = new List<Student>();
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class ExamsModel : PageModel
    {
        public List<Exam> Exams { get; set; } = new();
        public Dictionary<int, string> ClassNames { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            using var httpClient = new HttpClient();

            // Lấy danh sách bài thi
            var res = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/teacher/{teacherId}");
            Exams = JsonConvert.DeserializeObject<List<Exam>>(res);

            // Lấy danh sách lớp
            var classRes = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
            var classes = JsonConvert.DeserializeObject<List<Class>>(classRes) ?? new List<Class>();
            ClassNames = classes.ToDictionary(c => c.ClassId, c => c.Name);
        }
    }
}
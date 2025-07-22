using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using System.Linq;

namespace api.Pages.User.Teachers
{
    public class ExamDetailModel : PageModel
    {
        public Exam Exam { get; set; }
        public List<Result> Results { get; set; } = new();
        public Dictionary<int, string> StudentNames { get; set; } = new();

        public async Task OnGetAsync(int examId)
        {
            using var httpClient = new HttpClient();
            var examRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{examId}");
            Exam = JsonConvert.DeserializeObject<Exam>(examRes);

            var resultsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Result/exam/{examId}");
            Results = JsonConvert.DeserializeObject<List<Result>>(resultsRes);

            // Lấy danh sách StudentId cần thiết
            var studentIds = Results.Select(r => r.StudentId.ToString()).Distinct().ToList();

            // Lấy tất cả student (hoặc chỉ lấy theo id nếu có API hỗ trợ)
            var studentsRes = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Student/all");
            var students = JsonConvert.DeserializeObject<List<Student>>(studentsRes);

            // Map StudentId -> Name
            StudentNames = students.ToDictionary(s => s.StudentId, s => s.Name);

        }
    }
}
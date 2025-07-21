using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class ExamDetailModel : PageModel
    {
        public Exam Exam { get; set; }
        public List<Result> Results { get; set; } = new();

        public async Task OnGetAsync(int examId)
        {
            using var httpClient = new HttpClient();
            var examRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{examId}");
            Exam = JsonConvert.DeserializeObject<Exam>(examRes);

            var resultsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Result/exam/{examId}");
            Results = JsonConvert.DeserializeObject<List<Result>>(resultsRes);
        }
    }
}
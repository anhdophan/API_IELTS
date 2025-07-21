using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class QuestionsModel : PageModel
    {
        public List<Question> Questions { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            using var httpClient = new HttpClient();
            var res = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Question/teacher/{teacherId}");
            Questions = JsonConvert.DeserializeObject<List<Question>>(res);
        }
    }
}
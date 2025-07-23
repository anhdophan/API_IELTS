using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Pages.User.Teachers
{
    public class QuestionsModel : PageModel
    {
        public List<Question> Questions { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public bool OnlyMine { get; set; }

        public async Task OnGetAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");

            using var httpClient = new HttpClient();

            string url = OnlyMine
                ? $"https://api-ielts-cgn8.onrender.com/api/Question/teacher/{teacherId}"
                : $"https://api-ielts-cgn8.onrender.com/api/Question/all";

            var res = await httpClient.GetStringAsync(url);
            Questions = JsonConvert.DeserializeObject<List<Question>>(res);
        }
    }
}

using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.User.Teachers
{
    public class ClassDetailModel : PageModel
    {
        public Class Class { get; set; }
        public List<Student> Students { get; set; } = new();

        public async Task OnGetAsync(int classId)
        {
            using var httpClient = new HttpClient();
            var classRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{classId}");
            Class = JsonConvert.DeserializeObject<Class>(classRes);

            var studentsRes = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{classId}/students");
            Students = JsonConvert.DeserializeObject<List<Student>>(studentsRes);
        }
    }
}
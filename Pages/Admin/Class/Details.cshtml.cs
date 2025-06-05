using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;

namespace api.Pages.Admin.Classes
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public Class Class { get; set; }
        public string CourseName { get; set; }
        public string TeacherName { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var clsRes = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{id}");
            if (!clsRes.IsSuccessStatusCode) return NotFound();

            var json = await clsRes.Content.ReadAsStringAsync();
            Class = JsonConvert.DeserializeObject<Class>(json);

            var courseRes = await client.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Course/{Class.CourseId}");
            var course = JsonConvert.DeserializeObject<Course>(courseRes);
            CourseName = course?.Name ?? "N/A";

            if (Class.TeacherId != 0)
            {
                var teacherRes = await client.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{Class.TeacherId}");
                var teacher = JsonConvert.DeserializeObject<Teacher>(teacherRes);
                TeacherName = teacher?.Name;
            }
            return Page();
        }
    }
}

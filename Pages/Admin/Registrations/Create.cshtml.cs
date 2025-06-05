using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Text;
using Newtonsoft.Json;

namespace api.Pages.Admin.Registrations
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Registration Registration { get; set; } = new();

        [BindProperty]
        public Student Student { get; set; } = new();

        // Dùng để chọn Class từ danh sách lọc được
        [BindProperty]
        public int SelectedClassId { get; set; }

        public List<Course> CoursesWithClass { get; set; } = new();
        public List<Class> ClassOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            var courseRes = await client.GetAsync("http://localhost:5035/api/Course/all");
            courseRes.EnsureSuccessStatusCode();
            var courseJson = await courseRes.Content.ReadAsStringAsync();
            var allCourses = JsonConvert.DeserializeObject<List<Course>>(courseJson);

            foreach (var course in allCourses)
            {
                var classRes = await client.GetAsync($"http://localhost:5035/api/Registration/course/{course.CourseId}/classes");
                if (classRes.IsSuccessStatusCode)
                {
                    var classJson = await classRes.Content.ReadAsStringAsync();
                    var classes = JsonConvert.DeserializeObject<List<Class>>(classJson);
                    if (classes != null && classes.Any())
                    {
                        CoursesWithClass.Add(course);
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient();

            var request = new
            {
                Submit = true,
                Student = Student,
                CourseId = Registration.CourseId,
                ClassId = SelectedClassId
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync("http://localhost:5035/api/Registration/register-with-class", content);

            if (res.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to register student.");
            return Page();
        }
    }
}

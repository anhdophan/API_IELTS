using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Pages.Admin.Registrations
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Registration Registration { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetAsync($"http://localhost:5035/api/Registration/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            Registration = JsonConvert.DeserializeObject<Registration>(json);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateStudentAsync(int id)
        {
            var client = _clientFactory.CreateClient();

            var res = await client.GetAsync($"http://localhost:5035/api/Registration/{id}");
            if (!res.IsSuccessStatusCode) return RedirectToPage();

            var reg = JsonConvert.DeserializeObject<Registration>(await res.Content.ReadAsStringAsync());
            if (reg == null) return RedirectToPage();

            var student = new Student
            {
                StudentId = reg.StudentId,
                Name = "",
                Email = reg.Email,
                Username = reg.Email,
                Password = reg.Email + "123",
                PhoneNumber = "",
                ClassId = "",
                StudyingCourse = reg.CourseId.ToString(),
                Score = 0
            };

            var studentJson = JsonConvert.SerializeObject(student);
            var content = new StringContent(studentJson, System.Text.Encoding.UTF8, "application/json");
            var result = await client.PostAsync("http://localhost:5035/api/Student", content);

            StatusMessage = result.IsSuccessStatusCode
                ? $"Tạo học sinh từ đăng ký #{id} thành công."
                : $"Không thể tạo học sinh từ đăng ký #{id}.";

            return RedirectToPage(new { id });
        }
    }
}

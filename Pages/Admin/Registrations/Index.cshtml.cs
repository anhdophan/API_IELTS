using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Newtonsoft.Json;

namespace api.Pages.Admin.Registrations
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Registration> Registrations { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetStringAsync("http://localhost:5035/api/Registration");
            Registrations = JsonConvert.DeserializeObject<List<Registration>>(res) ?? new();
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
                Class = "",
                StudyingCourse = reg.CourseId.ToString(),
                Score = 0
            };

            var studentJson = JsonConvert.SerializeObject(student);
            var content = new StringContent(studentJson, System.Text.Encoding.UTF8, "application/json");
            var result = await client.PostAsync("http://localhost:5035/api/Student", content);

            StatusMessage = result.IsSuccessStatusCode
                ? $"Tạo học sinh từ đăng ký #{id} thành công."
                : $"Không thể tạo học sinh từ đăng ký #{id}.";

            return RedirectToPage();
        }
    }
}

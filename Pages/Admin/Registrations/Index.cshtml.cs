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

            // Lấy thông tin đăng ký
            var res = await client.GetAsync($"http://localhost:5035/api/Registration/{id}");
            if (!res.IsSuccessStatusCode) return RedirectToPage();

            var reg = JsonConvert.DeserializeObject<Registration>(await res.Content.ReadAsStringAsync());
            if (reg == null) return RedirectToPage();

            // Tạo Student từ thông tin đăng ký
            var student = new Student
            {
                StudentId = reg.StudentId == 0 ? new Random().Next(100000, 999999) : reg.StudentId,
                Name = "", // Nếu bạn lưu tên trong Registration thì lấy ra, còn không thì để rỗng
                Email = reg.Email,
                Username = reg.Email,
                Password = reg.Email + "123",
                PhoneNumber = "", // Nếu có thì lấy ra
                ClassId = "", // Nếu có thì lấy ra
                StudyingCourse = reg.CourseId.ToString(),
                Score = 0,
                Avatar = "https://ui-avatars.com/api/?name=Student" // hoặc link ảnh mặc định khác
            };

            var studentJson = JsonConvert.SerializeObject(student);
            var content = new StringContent(studentJson, System.Text.Encoding.UTF8, "application/json");
            var result = await client.PostAsync("http://localhost:5035/api/Student", content);

            StatusMessage = result.IsSuccessStatusCode
                ? $"Xác nhận đăng ký #{id} thành công, đã tạo học sinh."
                : $"Không thể xác nhận đăng ký #{id}.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.PostAsync($"http://localhost:5035/api/Registration/{id}/confirm", null);

            if (response.IsSuccessStatusCode)
                StatusMessage = "Xác nhận thành công!";
            else
                StatusMessage = "Xác nhận thất bại!";

            return RedirectToPage();
        }
    }
}

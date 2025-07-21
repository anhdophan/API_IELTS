using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace api.Pages.User.Teachers
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public Teacher Teacher { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(bool? edit)
        {
            IsEdit = edit == true;
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToPage("/User/Teachers/Login");

            using var httpClient = new HttpClient();
            var res = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}");
            if (!res.IsSuccessStatusCode)
            {
                ErrorMessage = "Không tìm thấy thông tin giảng viên.";
                Teacher = new Teacher();
                return Page();
            }
            var json = await res.Content.ReadAsStringAsync();
            Teacher = JsonConvert.DeserializeObject<Teacher>(json);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = HttpContext.Session.GetString("TeacherId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToPage("/User/Teachers/Login");

            Teacher.TeacherId = int.Parse(teacherId);

            using var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(Teacher), System.Text.Encoding.UTF8, "application/json");
            var res = await httpClient.PutAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}", content);

            if (res.IsSuccessStatusCode)
            {
                SuccessMessage = "Cập nhật thông tin thành công!";
                IsEdit = false;
            }
            else
            {
                ErrorMessage = "Cập nhật thất bại. Vui lòng thử lại.";
            }

            return await OnGetAsync(IsEdit);
        }
    }
}
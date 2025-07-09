using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace api.Pages.User.Student
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public dynamic StudentInfo { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            Username = Request.Form["username"];
            Password = Request.Form["password"];

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
                return Page();
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://api-ielts-cgn8.onrender.com/api/Student/all");
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể kết nối tới hệ thống.");
                    return Page();
                }

                var json = await response.Content.ReadAsStringAsync();
                // Dữ liệu trả về có thể là List hoặc Dictionary
                List<JObject> students = new List<JObject>();
                try
                {
                    students = JArray.Parse(json).ToObject<List<JObject>>();
                }
                catch
                {
                    var dict = JObject.Parse(json);
                    foreach (var item in dict.Properties())
                    {
                        students.Add((JObject)item.Value);
                    }
                }

                foreach (var student in students)
                {
                    if (student["Username"]?.ToString() == Username && student["Password"]?.ToString() == Password)
                    {
                        // Đăng nhập thành công, lưu thông tin vào TempData/Session nếu muốn
                        StudentInfo = student;
                        // Ví dụ lưu vào TempData
                        TempData["StudentId"] = student["StudentId"]?.ToString();
                        TempData["StudentName"] = student["Name"]?.ToString();
                        TempData["StudentAvatar"] = student["Avatar"]?.ToString();
                        TempData["StudentEmail"] = student["Email"]?.ToString();
                        // ... các trường khác nếu cần

                        // Chuyển hướng sang trang cá nhân hoặc dashboard
                        return RedirectToPage("/User/Student/Index");
                    }
                }

                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                return Page();
            }
        }
    }
}
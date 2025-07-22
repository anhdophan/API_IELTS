using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace api.Pages.User.Teachers
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
            var isTeacherLogin = !string.IsNullOrEmpty(Request.Form["teacherLogin"]);

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
                return Page();
            }

            using (var httpClient = new HttpClient())
            {
              
                    // Đăng nhập cho Teacher
                    var response = await httpClient.GetAsync("https://api-ielts-cgn8.onrender.com/api/Teacher/all");
                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "Không thể kết nối tới hệ thống.");
                        return Page();
                    }
                    var json = await response.Content.ReadAsStringAsync();
                    List<JObject> teachers = new();
                    try
                    {
                        teachers = JArray.Parse(json).ToObject<List<JObject>>();
                    }
                    catch
                    {
                        var dict = JObject.Parse(json);
                        foreach (var item in dict.Properties())
                        {
                            teachers.Add((JObject)item.Value);
                        }
                    }
                    foreach (var teacher in teachers)
                    {
                        var storedUsername = teacher["username"]?.ToString();
                        var storedPassword = teacher["password"]?.ToString();

                        if (storedUsername == Username && storedPassword == Password)
                        {
                            // Gán session cho Teacher
                            HttpContext.Session.SetString("TeacherId", teacher["teacherId"]?.ToString() ?? "");
                            HttpContext.Session.SetString("TeacherName", teacher["name"]?.ToString() ?? "");
                            HttpContext.Session.SetString("TeacherAvatar", teacher["avatar"]?.ToString() ?? "");
                            HttpContext.Session.SetString("TeacherEmail", teacher["email"]?.ToString() ?? "");

                            return RedirectToPage("/User/Teachers/Index");
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                    return Page();
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Firebase.Database.Query;

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
                            var teacherId = teacher["teacherId"]?.ToString() ?? "";
                            HttpContext.Session.SetString("TeacherName", teacher["name"]?.ToString() ?? "");
                            HttpContext.Session.SetString("TeacherAvatar", teacher["avatar"]?.ToString() ?? "");
                            HttpContext.Session.SetString("TeacherEmail", teacher["email"]?.ToString() ?? "");
                            // ✅ Gọi API lấy danh sách lớp giáo viên đang dạy
                            var classResponse = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}/classes");
                        if (classResponse.IsSuccessStatusCode)
                        {
                            var classJson = await classResponse.Content.ReadAsStringAsync();
                            var classList = JArray.Parse(classJson);

                            var classIdList = new List<string>();
                            foreach (var c in classList)
                            {
                                var classId = c["classId"]?.ToString();
                                if (!string.IsNullOrEmpty(classId))
                                    classIdList.Add(classId);
                            }

                            // ✅ Lưu vào Session: danh sách classId của giáo viên
                            HttpContext.Session.SetString("TeacherClasses", Newtonsoft.Json.JsonConvert.SerializeObject(classIdList));

                            // ✅ Lưu class đầu tiên để dùng cho chat nhóm mặc định
                            if (classIdList.Count > 0)
                                HttpContext.Session.SetString("TeacherClass", classIdList[0]);
                                var firebaseClient = new Firebase.Database.FirebaseClient("https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/");

                                await firebaseClient
                                    .Child("users")
                                    .Child(teacherId)
                                    .PutAsync(new
                                    {
                                        userId = teacherId,
                                        name = teacher["name"]?.ToString(),
                                        email = teacher["email"]?.ToString(),
                                        avatar = teacher["avatar"]?.ToString(),
                                        role = "teacher",
                                        classIds = classIdList
                                    });
                            }


                            return RedirectToPage("/User/Teachers/Index");
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                    return Page();
            }
        }
    }
}
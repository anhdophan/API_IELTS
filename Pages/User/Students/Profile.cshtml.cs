using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace api.Pages.User.Students
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public Student Student { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(bool? edit)
        {
            IsEdit = edit == true;
            var studentIdStr = HttpContext.Session.GetString("StudentId") ?? TempData["StudentId"]?.ToString();
            if (string.IsNullOrEmpty(studentIdStr))
                return RedirectToPage("/User/Students/Login");

            using var httpClient = new HttpClient();
            var res = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{studentIdStr}");
            if (!res.IsSuccessStatusCode)
            {
                ErrorMessage = "Không tìm thấy thông tin sinh viên.";
                Student = new Student();
                return Page();
            }
            var json = await res.Content.ReadAsStringAsync();
            Student = JsonConvert.DeserializeObject<Student>(json);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile AvatarUpload)
        {
            IsEdit = true;
            var studentIdStr = HttpContext.Session.GetString("StudentId") ?? TempData["StudentId"]?.ToString();
            if (string.IsNullOrEmpty(studentIdStr))
                return RedirectToPage("/User/Students/Login");

            Student.StudentId = int.Parse(studentIdStr);

            // Xử lý upload avatar (giả sử bạn có API hoặc cloud để upload, ở đây chỉ demo)
            if (AvatarUpload != null && AvatarUpload.Length > 0)
            {
                // TODO: Upload lên cloud và lấy link, ở đây chỉ demo lấy tên file
                Student.Avatar = "/uploads/" + Path.GetFileName(AvatarUpload.FileName);
            }

            // Xử lý đổi mật khẩu
            var oldPassword = Request.Form["OldPassword"];
            var newPassword = Request.Form["NewPassword"];
            var confirmPassword = Request.Form["ConfirmPassword"];
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (string.IsNullOrEmpty(oldPassword) || oldPassword != Student.Password)
                {
                    ErrorMessage = "Mật khẩu hiện tại không đúng.";
                    return await OnGetAsync(true);
                }
                if (newPassword != confirmPassword)
                {
                    ErrorMessage = "Mật khẩu mới không khớp.";
                    return await OnGetAsync(true);
                }
                Student.Password = newPassword;
            }

            using var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(Student), System.Text.Encoding.UTF8, "application/json");
            var res = await httpClient.PutAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{studentIdStr}", content);

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
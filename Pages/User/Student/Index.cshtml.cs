using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace api.Pages.User.Student
{
    public class IndexModel : PageModel
    {
        public List<ExamInfo> Exams { get; set; } = new();
        public string StudentName { get; set; }
        public string StudentAvatar { get; set; }

       public async Task<IActionResult> OnGetAsync()
{
    var studentId = HttpContext.Session.GetString("StudentId");
    var studentName = HttpContext.Session.GetString("StudentName");
    var studentAvatar = HttpContext.Session.GetString("StudentAvatar");
    var classIdStr = HttpContext.Session.GetString("StudentClass");

    if (string.IsNullOrEmpty(classIdStr))
    {
        return RedirectToPage("/User/Student/Login");
    }

    StudentName = studentName ?? "Student";
    StudentAvatar = studentAvatar ?? "";

    int classId = int.Parse(classIdStr);

    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/class/{classId}");
    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        Exams = JsonConvert.DeserializeObject<List<ExamInfo>>(json) ?? new List<ExamInfo>();
    }

    return Page();
}


        public class ExamInfo
        {
            public int ExamId { get; set; }
            public string Title { get; set; }
            public DateTime ExamDate { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using System.Linq;

namespace api.Pages.User.Students
{
    public class ExamPageModel : PageModel
{
    public Exam Exam { get; set; }
    public Result Result { get; set; }
    public bool IsExpired { get; set; }
    public bool IsNotStarted { get; set; }

    public async Task<IActionResult> OnGetAsync(int examId)
    {
        var studentIdStr = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentIdStr))
            return RedirectToPage("/User/Students/Login");

        int studentId = int.Parse(studentIdStr);

        using var httpClient = new HttpClient();

        var examRes = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{examId}");
        if (!examRes.IsSuccessStatusCode)
            return NotFound("Không tìm thấy bài thi.");

        var examJson = await examRes.Content.ReadAsStringAsync();
        Exam = JsonConvert.DeserializeObject<Exam>(examJson);

        var resultRes = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Result/student/{studentId}/exam/{examId}");
        if (resultRes.IsSuccessStatusCode)
        {
            var resultJson = await resultRes.Content.ReadAsStringAsync();
            Result = JsonConvert.DeserializeObject<Result>(resultJson);
        }

        var nowVN = DateTime.UtcNow.AddHours(7); // Giờ Việt Nam
        IsExpired = Exam.EndTime < nowVN;
        IsNotStarted = Exam.StartTime > nowVN;

        return Page();
    }
}

}

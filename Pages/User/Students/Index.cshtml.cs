using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    namespace api.Pages.User.Students
    {
        public class IndexModel : PageModel
        {
            public List<ExamInfo> Exams { get; set; } = new();
            // public List<ClassScheduleInfo> ClassSchedule { get; set; } = new(); // Remove this, JavaScript will handle
            public string StudentName { get; set; }
            public string StudentAvatar { get; set; }
            public string StudentClassId { get; set; } // New property to pass classId to JS

            public async Task<IActionResult> OnGetAsync()
            {
                var studentId = HttpContext.Session.GetString("StudentId");
                var studentName = HttpContext.Session.GetString("StudentName");
                var studentAvatar = HttpContext.Session.GetString("StudentAvatar");
                var classIdStr = HttpContext.Session.GetString("StudentClass");

                if (string.IsNullOrEmpty(classIdStr))
                {
                    return RedirectToPage("/User/Students/Login");
                }

                StudentName = studentName ?? "Student";
                StudentAvatar = studentAvatar ?? "";
                StudentClassId = classIdStr; // Assign to the new property

                int classId = int.Parse(classIdStr);

                using var httpClient = new HttpClient();

                // Fetch Exam data (keep this as it's not related to the weekly schedule)
                try
                {
                    var examResponse = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/class/{classId}");
                    if (examResponse.IsSuccessStatusCode)
                    {
                        var json = await examResponse.Content.ReadAsStringAsync();
                        Exams = JsonConvert.DeserializeObject<List<ExamInfo>>(json) ?? new List<ExamInfo>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching exam data: {ex.Message}");
                }

                // Removed the ClassSchedule fetching logic from OnGetAsync
                // This will now be handled by JavaScript using Fetch API on the client side.

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

            // DTOs needed by JavaScript for fetching (though not directly used by C# OnGet anymore for schedule)
            // Keep them here for reference or if you decide to pre-fetch some data
            public class RawScheduleEntry
            {
                public DateTime Date { get; set; }
                public string DayOfWeek { get; set; }
                public string StartTime { get; set; }
                public string EndTime { get; set; }
            }

            public class ClassScheduleInfo
            {
                public string CourseName { get; set; }
                public string DayOfWeek { get; set; }
                public DateTime Date { get; set; }
                public string StartTime { get; set; }
                public string EndTime { get; set; }
                public string Room { get; set; }
                public string TeacherName { get; set; }
            }

            public class ClassDetail
            {
                public string ClassId { get; set; }
                [JsonProperty("room")] // Assume 'room' is the field name for room in Firebase Class object
                public string Room { get; set; }
                public string ClassName { get; set; } // Keep this if 'room' might be 'className' sometimes
                public int CourseId { get; set; }
                public int TeacherId { get; set; }
            }

            public class CourseInfo
            {
                public int CourseId { get; set; }
                [JsonProperty("name")] // Use JsonProperty to map 'name' from API to CourseName
                public string CourseName { get; set; }
            }

            public class TeacherInfo
            {
                public int TeacherId { get; set; }
                [JsonProperty("name")] // Use JsonProperty to map 'name' from API to TeacherName
                public string TeacherName { get; set; }
            }
        }
    }
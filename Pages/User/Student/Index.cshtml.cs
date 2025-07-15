
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
            public List<ClassScheduleInfo> ClassSchedule { get; set; } = new(); // New property for class schedule
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
                    // Redirect to login if classId is not in session
                    return RedirectToPage("/User/Student/Login");
                }

                StudentName = studentName ?? "Student";
                StudentAvatar = studentAvatar ?? "";

                int classId = int.Parse(classIdStr);
                string className = ""; // Will fetch this along with other class details
                int courseId = 0; // Will fetch this to get course name
                int teacherId = 0; // Will fetch this to get teacher name

                using var httpClient = new HttpClient();

                // 1. Fetch Class details to get CourseId, TeacherId, and Room
                try
                {
                    var classResponse = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{classId}");
                    if (classResponse.IsSuccessStatusCode)
                    {
                        var classJson = await classResponse.Content.ReadAsStringAsync();
                        var classObj = JsonConvert.DeserializeObject<ClassDetail>(classJson);
                        if (classObj != null)
                        {
                            className = classObj.ClassName; // Assuming Class has a ClassName property
                            courseId = classObj.CourseId;
                            teacherId = classObj.TeacherId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't prevent page load if class details fail
                    Console.WriteLine($"Error fetching class details: {ex.Message}");
                }


                // 2. Fetch Course details to get CourseName
                string courseName = "N/A";
                if (courseId > 0)
                {
                    try
                    {
                        var courseResponse = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Course/{courseId}");
                        if (courseResponse.IsSuccessStatusCode)
                        {
                            var courseJson = await courseResponse.Content.ReadAsStringAsync();
                            var courseObj = JsonConvert.DeserializeObject<CourseInfo>(courseJson);
                            if (courseObj != null)
                            {
                                courseName = courseObj.CourseName;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching course details: {ex.Message}");
                    }
                }

                // 3. Fetch Teacher details to get TeacherName
                string teacherName = "N/A";
                if (teacherId > 0)
                {
                    try
                    {
                        var teacherResponse = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{teacherId}");
                        if (teacherResponse.IsSuccessStatusCode)
                        {
                            var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
                            var teacherObj = JsonConvert.DeserializeObject<TeacherInfo>(teacherJson);
                            if (teacherObj != null)
                            {
                                teacherName = teacherObj.TeacherName; // Assuming Teacher has a TeacherName property
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching teacher details: {ex.Message}");
                    }
                }

                // 4. Fetch Exam data (existing logic)
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


                // 5. Fetch Class Schedule data
                try
                {
                    var scheduleResponse = await httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{classId}/studydays");
                    if (scheduleResponse.IsSuccessStatusCode)
                    {
                        var json = await scheduleResponse.Content.ReadAsStringAsync();
                        var rawSchedule = JsonConvert.DeserializeObject<List<RawScheduleEntry>>(json) ?? new List<RawScheduleEntry>();

                        ClassSchedule = rawSchedule.Select(rs => new ClassScheduleInfo
                        {
                            CourseName = courseName, // Use the fetched course name
                            DayOfWeek = rs.DayOfWeek,
                            Date = rs.Date,
                            StartTime = rs.StartTime,
                            EndTime = rs.EndTime,
                            Room = className, // Assuming ClassName is actually Room in this context or you have a separate Room property
                            TeacherName = teacherName // Use the fetched teacher name
                        }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching class schedule: {ex.Message}");
                    // Optionally set an error message for the user
                    // ModelState.AddModelError(string.Empty, "Không thể tải lịch học. Vui lòng thử lại sau.");
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

            // DTO to match the structure of the /api/Class/{classId}/studydays response
            public class RawScheduleEntry
            {
                public DateTime Date { get; set; }
                public string DayOfWeek { get; set; }
                public string StartTime { get; set; } // Time as string (e.g., "08:00")
                public string EndTime { get; set; }   // Time as string (e.g., "10:00")
            }

            // Combined DTO for displaying in the table
            public class ClassScheduleInfo
            {
                public string CourseName { get; set; }
                public string DayOfWeek { get; set; }
                public DateTime Date { get; set; }
                public string StartTime { get; set; }
                public string EndTime { get; set; }
                public string Room { get; set; } // This needs to come from ClassDetails if it's not className
                public string TeacherName { get; set; }
            }

            // DTO for Class details from /api/Class/{classId}
            public class ClassDetail
            {
                public string ClassId { get; set; }
                public string ClassName { get; set; } // Assuming ClassName can be used as Room, or add a Room property
                public int CourseId { get; set; }
                public int TeacherId { get; set; }
                // Add other properties you might need from the Class object
            }

            // DTO for Course details from /api/Course/{courseId}
            public class CourseInfo
            {
                public int CourseId { get; set; }
                public string CourseName { get; set; }
                // Add other properties if needed
            }

            // DTO for Teacher details from /api/Teacher/{teacherId}
            public class TeacherInfo
            {
                public int TeacherId { get; set; }
                public string TeacherName { get; set; }
                // Add other properties if needed
            }
        }
    }

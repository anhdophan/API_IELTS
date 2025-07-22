using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Services;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        [HttpGet("search")]
        public async Task<ActionResult<List<Teacher>>> SearchTeachersAsync([FromQuery] string name, [FromQuery] string email)
        {
            var all = await GetAllTeachersInternal();

            var filtered = all.Where(t =>
                (string.IsNullOrEmpty(name) || t.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(email) || t.Email.Contains(email, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            return Ok(filtered);
        }

        private async Task<List<Teacher>> GetAllTeachersInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Teachers.json";
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Teacher>>(json);
                if (dict != null) return dict.Values.Where(t => t != null).ToList();
            }
            catch { }

            try
            {
                var list = JsonConvert.DeserializeObject<List<Teacher>>(json);
                if (list != null) return list.Where(t => t != null).ToList();
            }
            catch { }

            throw new Exception("Unable to parse teacher data.");
        }


        [HttpPost]
        public async Task<IActionResult> CreateTeacherAsync([FromBody] Teacher teacher)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await firebaseClient
                .Child("Teachers")
                .Child(teacher.TeacherId.ToString())
                .OnceSingleAsync<Teacher>();

            if (existing != null)
                return Conflict($"Teacher with ID {teacher.TeacherId} already exists.");

            await firebaseClient
                .Child("Teachers")
                .Child(teacher.TeacherId.ToString())
                .PutAsync(teacher);

            return Ok(teacher);
        }

        [HttpPut("{teacherId}")]
        public async Task<IActionResult> UpdateTeacherAsync(string teacherId, [FromBody] Teacher teacher)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (teacherId != teacher.TeacherId.ToString())
                return BadRequest("Teacher ID mismatch.");

            await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .PutAsync(teacher);

            return Ok(teacher);
        }

        [HttpDelete("{teacherId}")]
        public async Task<IActionResult> DeleteTeacherAsync(string teacherId)
        {
            await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .DeleteAsync();

            return Ok();
        }

        [HttpGet("{teacherId}")]
        public async Task<ActionResult<Teacher>> GetTeacherAsync(string teacherId)
        {
            var teacher = await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .OnceSingleAsync<Teacher>();

            if (teacher == null) return NotFound();
            return Ok(teacher);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Teacher>>> GetAllTeachersAsync()
        {
            try
            {
                var list = await GetAllTeachersInternal();
                return Ok(list);
            }
            catch
            {
                return BadRequest("Unable to parse Teachers from Firebase.");
            }
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<Teacher>>> FilterTeachersAsync([FromQuery] string? email, [FromQuery] string? name)
        {
            var all = await GetAllTeachersInternal();

            var result = all.Where(t =>
                (string.IsNullOrEmpty(email) || t.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(name) || t.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            return Ok(result);
        }

        [HttpGet("{teacherId}/classes")]
        public async Task<ActionResult<List<Class>>> GetClassesOfTeacher(string teacherId)
        {
            var teacher = await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .OnceSingleAsync<Teacher>();

            if (teacher == null || teacher.ClassIds == null)
                return Ok(new List<Class>());

            var result = new List<Class>();
            foreach (var id in teacher.ClassIds)
            {
                var cls = await firebaseClient.Child("Classes").Child(id.ToString()).OnceSingleAsync<Class>();
                if (cls != null) result.Add(cls);
            }
            return Ok(result);
        }

        [HttpGet("{teacherId}/courses")]
        public async Task<ActionResult<List<Course>>> GetCoursesOfTeacher(string teacherId)
        {
            var teacher = await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .OnceSingleAsync<Teacher>();

            if (teacher == null || teacher.CourseIds == null)
                return Ok(new List<Course>());

            var result = new List<Course>();
            foreach (var id in teacher.CourseIds)
            {
                var course = await firebaseClient.Child("Courses").Child(id.ToString()).OnceSingleAsync<Course>();
                if (course != null) result.Add(course);
            }
            return Ok(result);
        }

        [HttpGet("{teacherId}/exams")]
        public async Task<ActionResult<List<Exam>>> GetExamsOfTeacher(string teacherId)
        {
            var teacher = await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .OnceSingleAsync<Teacher>();

            if (teacher == null || teacher.ExamIds == null)
                return Ok(new List<Exam>());

            var result = new List<Exam>();
            foreach (var id in teacher.ExamIds)
            {
                var exam = await firebaseClient.Child("Exams").Child(id.ToString()).OnceSingleAsync<Exam>();
                if (exam != null) result.Add(exam);
            }
            return Ok(result);
        }
        
        [HttpGet("{teacherId}/schedule")]
        public async Task<ActionResult<List<object>>> GetTeachingSchedule(string teacherId)
        {
            var teacher = await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .OnceSingleAsync<Teacher>();

            if (teacher == null || teacher.ClassIds == null)
                return Ok(new List<object>());

            var scheduleList = new List<object>();

            foreach (var classId in teacher.ClassIds)
            {
                var cls = await firebaseClient.Child("Classes").Child(classId.ToString()).OnceSingleAsync<Class>();
                if (cls == null || cls.Schedule == null) continue;

                // Duyệt từng ngày trong khoảng thời gian của lớp
                for (var date = cls.StartDate.Date; date <= cls.EndDate.Date; date = date.AddDays(1))
                {
                    foreach (var sched in cls.Schedule)
                    {
                        if (Enum.TryParse<DayOfWeek>(sched.DayOfWeek, true, out var dow) && date.DayOfWeek == dow)
                        {
                            scheduleList.Add(new
                            {
                                ClassId = cls.ClassId,
                                ClassName = cls.Name,
                                Date = date,
                                DayOfWeek = sched.DayOfWeek,
                                StartTime = sched.StartTime,
                                EndTime = sched.EndTime,
                                Room = cls.Name // Tên lớp cũng là tên phòng học
                            });
                        }
                    }
                }
            }

            return Ok(scheduleList.OrderBy(s => ((DateTime)s.GetType().GetProperty("Date").GetValue(s))).ToList());
        }

        [HttpGet("{teacherId}/examschedule")]
        public async Task<ActionResult<List<object>>> GetTeachingExamSchedule(string teacherId)
        {
            var teacher = await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .OnceSingleAsync<Teacher>();

            if (teacher == null || teacher.ClassIds == null)
                return Ok(new List<object>());

            var allExams = new List<object>();

            foreach (var classId in teacher.ClassIds)
            {
                var httpClient = new HttpClient();
                var examJson = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/class/{classId}");
                List<Exam> exams;
                try
                {
                    exams = JsonConvert.DeserializeObject<List<Exam>>(examJson);
                }
                catch
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Exam>>(examJson);
                    exams = dict?.Values.ToList() ?? new List<Exam>();
                }
                if (exams != null)
                {
                    foreach (var exam in exams)
                    {
                        allExams.Add(new
                        {
                            ExamId = exam.ExamId,
                            Title = exam.Title,
                            ExamDate = exam.ExamDate,
                            StartTime = exam.StartTime,
                            EndTime = exam.EndTime,
                            ClassId = exam.IdClass,
                            Room = exam.ClassName ?? exam.IdClass.ToString() // Tên lớp cũng là tên phòng học
                        });
                    }
                }
            }

            return Ok(allExams.OrderBy(e => ((DateTime)e.GetType().GetProperty("ExamDate").GetValue(e))).ToList());
        }
    }
}

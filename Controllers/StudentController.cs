using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Models;
using api.Services;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create student
        [HttpPost]
        public async Task<IActionResult> CreateStudentAsync([FromBody] Student student)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await firebaseClient
                .Child("Students")
                .Child(student.StudentId.ToString())
                .OnceSingleAsync<Student>();

            if (existing != null)
                return Conflict($"Student with ID {student.StudentId} already exists.");

            await firebaseClient
                .Child("Students")
                .Child(student.StudentId.ToString())
                .PutAsync(student);

            return Ok(student);
        }

        // Update student
        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudentAsync(string studentId, [FromBody] Student student)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (studentId != student.StudentId.ToString())
                return BadRequest("Student ID mismatch.");

            await firebaseClient
                .Child("Students")
                .Child(studentId)
                .PutAsync(student);

            return Ok(student);
        }

        // Delete student
        [HttpDelete("{studentId}")]
        public async Task<IActionResult> DeleteStudentAsync(string studentId)
        {
            await firebaseClient
                .Child("Students")
                .Child(studentId)
                .DeleteAsync();

            return Ok();
        }

        // Get student by ID
        [HttpGet("{studentId}")]
        public async Task<ActionResult<Student>> GetStudentAsync(string studentId)
        {
            var student = await firebaseClient
                .Child("Students")
                .Child(studentId)
                .OnceSingleAsync<Student>();

            if (student == null) return NotFound();
            return Ok(student);
        }

        // Get all students
        [HttpGet("all")]
        public async Task<ActionResult<List<Student>>> GetAllStudentsAsync()
        {
            try
            {
                var students = await GetAllStudentsInternal();
                return Ok(students);
            }
            catch
            {
                return BadRequest("Failed to retrieve students.");
            }
        }

        // Filter by Class name
        [HttpGet("class")]
        public async Task<ActionResult<List<Student>>> GetStudentsByClassAsync([FromQuery] string className)
        {
            var all = await GetAllStudentsInternal();
            var filtered = all
                .Where(s => s.ClassId != null && s.ClassId.Equals(className, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(filtered);
        }

        // Filter by email
        [HttpGet("email")]
        public async Task<ActionResult<Student>> GetStudentByEmailAsync([FromQuery] string email)
        {
            var all = await GetAllStudentsInternal();
            var student = all.FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (student == null) return NotFound();
            return Ok(student);
        }

        // Filter by CourseId, Status, or StudentId
        [HttpGet("filter")]
        public async Task<ActionResult<List<Student>>> FilterStudentsAsync([FromQuery] int? courseId, [FromQuery] RegistrationStatus? status, [FromQuery] int? studentId)
        {
            var students = await GetAllStudentsInternal();
            var registrations = await firebaseClient.Child("Registrations").OnceAsync<Registration>();
            var regs = registrations.Select(r => r.Object).ToList();

            if (studentId.HasValue)
                students = students.Where(s => s.StudentId == studentId.Value).ToList();

            if (courseId.HasValue)
            {
                var validStudentIds = regs.Where(r => r.CourseId == courseId.Value).Select(r => r.StudentId).Distinct();
                students = students.Where(s => validStudentIds.Contains(s.StudentId)).ToList();
            }

            if (status.HasValue)
            {
                var validStudentIds = regs.Where(r => r.Status == status.Value).Select(r => r.StudentId).Distinct();
                students = students.Where(s => validStudentIds.Contains(s.StudentId)).ToList();
            }

            return Ok(students);
        }

        // Get exams that student has completed
        [HttpGet("{studentId}/exams")]
        public async Task<ActionResult<List<Result>>> GetExamsDoneAsync(string studentId)
        {
            var results = await firebaseClient.Child("Results").OnceAsync<Result>();

            var filtered = results
                .Select(r => r.Object)
                .Where(r => r.StudentId.ToString() == studentId)
                .ToList();

            return Ok(filtered);
        }

        // Update student score
        [HttpPost("{studentId}/update-score")]
        public async Task<IActionResult> UpdateStudentScoreAsync(string studentId)
        {
            // Lấy student
            var student = await firebaseClient
                .Child("Students")
                .Child(studentId)
                .OnceSingleAsync<Student>();
            if (student == null)
                return NotFound("Student not found.");

            // Lấy tất cả kết quả Result của student này
            var results = await firebaseClient.Child("Results").OnceAsync<Result>();
            var studentResults = results
                .Select(r => r.Object)
                .Where(r => r.StudentId.ToString() == studentId)
                .ToList();

            if (!studentResults.Any())
                return BadRequest("Student has no exam results.");

            // Tính điểm từng bài thi, điểm trung bình (số và %)
            double totalScore = studentResults.Sum(r => r.Score);
            double totalMax = studentResults.Sum(r => r.TotalScore);
            double avgScore = studentResults.Average(r => r.Score);
            double avgPercent = totalMax > 0 ? (totalScore / totalMax) * 100 : 0;

            // Lưu điểm trung bình vào trường Score của Student
            student.Score = avgScore;

            // (Có thể lưu thêm điểm % vào một trường khác nếu muốn, ví dụ: student.AvgPercent = avgPercent;)

            // Cập nhật lại Student
            await firebaseClient
                .Child("Students")
                .Child(studentId)
                .PutAsync(student);

            // Trả về thông tin điểm
            return Ok(new
            {
                StudentId = student.StudentId,
                Name = student.Name,
                AvgScore = avgScore,
                AvgPercent = avgPercent,
                ExamScores = studentResults.Select(r => new { r.ExamId, r.Score, r.TotalScore })
            });
        }

        // Private helper to get all students
        private async Task<List<Student>> GetAllStudentsInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Students.json";
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);

            try
            {
                var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Student>>(json);
                if (dict != null)
                    return dict.Values.ToList();
            }
            catch { }

            try
            {
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Student>>(json);
                if (list != null)
                    return list;
            }
            catch { }

            throw new Exception("Unable to parse student data.");
        }
    }
}

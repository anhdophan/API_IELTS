using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using api.Models;
using api.Services;
using Firebase.Database.Query;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create a new student
        [HttpPost]
        public async Task<IActionResult> CreateStudentAsync([FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Check if student with same ID exists
                var existing = await firebaseClient.Child("Students").Child(student.StudentId.ToString()).OnceSingleAsync<Student>();
                if (existing != null)
                    return Conflict("Student with this ID already exists.");

                await firebaseClient
                    .Child("Students")
                    .Child(student.StudentId.ToString())
                    .PutAsync(student);

                return CreatedAtAction(nameof(GetStudentAsync), new { studentId = student.StudentId }, student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating student: {ex.Message}");
            }
        }

        // Update student info
        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudentAsync(string studentId, [FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (studentId != student.StudentId.ToString())
                return BadRequest("Student ID mismatch.");

            try
            {
                var existing = await firebaseClient.Child("Students").Child(studentId).OnceSingleAsync<Student>();
                if (existing == null)
                    return NotFound("Student not found.");

                await firebaseClient
                    .Child("Students")
                    .Child(studentId)
                    .PutAsync(student);

                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating student: {ex.Message}");
            }
        }

        // Delete a student
        [HttpDelete("{studentId}")]
        public async Task<IActionResult> DeleteStudentAsync(string studentId)
        {
            try
            {
                var existing = await firebaseClient.Child("Students").Child(studentId).OnceSingleAsync<Student>();
                if (existing == null)
                    return NotFound("Student not found.");

                await firebaseClient
                    .Child("Students")
                    .Child(studentId)
                    .DeleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting student: {ex.Message}");
            }
        }

        // Get a student by ID
        [HttpGet("{studentId}")]
        public async Task<ActionResult<Student>> GetStudentAsync(string studentId)
        {
            try
            {
                var student = await firebaseClient
                    .Child("Students")
                    .Child(studentId)
                    .OnceSingleAsync<Student>();
                if (student == null) return NotFound();

                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching student: {ex.Message}");
            }
        }

        // Get all students
        // Query param: format=list or dictionary (default list)
        [HttpGet]
        public async Task<IActionResult> GetAllStudentsAsync([FromQuery] string format = "list")
        {
            try
            {
                var students = await firebaseClient
                        .Child("Students")
                        .OnceAsync<Student>();

                if (format.ToLower() == "dictionary")
                {
                    var dict = students.ToDictionary(s => s.Key, s => s.Object);
                    return Ok(dict);
                }
                else
                {
                    var studentList = students.Select(s => s.Object).ToList();
                    return Ok(studentList);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching students: {ex.Message}");
            }
        }

        // Get students by Class name
        [HttpGet("by-class/{className}")]
        public async Task<IActionResult> GetStudentsByClassAsync(string className)
        {
            try
            {
                var students = await firebaseClient
                    .Child("Students")
                    .OnceAsync<Student>();

                var filtered = students
                    .Where(s => s.Object.Class != null && s.Object.Class.Equals(className, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Object)
                    .ToList();

                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching students by class: {ex.Message}");
            }
        }

        // Get student by email
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetStudentByEmailAsync(string email)
        {
            try
            {
                var students = await firebaseClient
                    .Child("Students")
                    .OnceAsync<Student>();

                var student = students
                    .Select(s => s.Object)
                    .FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (student == null) return NotFound();

                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching student by email: {ex.Message}");
            }
        }

        // Filter students by CourseId or RegistrationStatus or StudentId (query params)
        // Note: This assumes you have a way to join Registration and Student data.
        // For demo, we fetch all students and registrations then filter accordingly.
        [HttpGet("filter")]
        public async Task<IActionResult> FilterStudentsAsync([FromQuery] int? courseId, [FromQuery] RegistrationStatus? status, [FromQuery] int? studentId)
        {
            try
            {
                var students = await firebaseClient.Child("Students").OnceAsync<Student>();
                var registrations = await firebaseClient.Child("Registrations").OnceAsync<Registration>();

                // Join student with their registrations
                var studentList = students.Select(s => s.Object).ToList();
                var registrationList = registrations.Select(r => r.Object).ToList();

                // Filter by studentId
                if (studentId.HasValue)
                    studentList = studentList.Where(s => s.StudentId == studentId.Value).ToList();

                // Filter by courseId
                if (courseId.HasValue)
                {
                    var regStudentsByCourse = registrationList
                        .Where(r => r.CourseId == courseId.Value)
                        .Select(r => r.StudentId)
                        .Distinct()
                        .ToList();
                    studentList = studentList.Where(s => regStudentsByCourse.Contains(s.StudentId)).ToList();
                }

                // Filter by status
                if (status.HasValue)
                {
                    var regStudentsByStatus = registrationList
                        .Where(r => r.Status == status.Value)
                        .Select(r => r.StudentId)
                        .Distinct()
                        .ToList();
                    studentList = studentList.Where(s => regStudentsByStatus.Contains(s.StudentId)).ToList();
                }

                return Ok(studentList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error filtering students: {ex.Message}");
            }
        }

        // List exams a student has done (based on Result model)
        [HttpGet("{studentId}/exams-done")]
        public async Task<IActionResult> GetExamsDoneAsync(string studentId)
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var doneExams = results
                    .Where(r => r.Object.StudentId.ToString() == studentId)
                    .Select(r => r.Object)
                    .ToList();

                return Ok(doneExams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching exams done: {ex.Message}");
            }
        }
    }
}

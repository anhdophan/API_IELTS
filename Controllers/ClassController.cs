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
    public class ClassController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create Class
        [HttpPost]
        public async Task<IActionResult> CreateClassAsync([FromBody] Class cls)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await firebaseClient
                .Child("Classes")
                .Child(cls.ClassId.ToString())
                .OnceSingleAsync<Class>();

            if (existing != null)
                return Conflict($"Class with ID {cls.ClassId} already exists.");

            // Save new class
            await firebaseClient
                .Child("Classes")
                .Child(cls.ClassId.ToString())
                .PutAsync(cls);

            // Update Course to include this ClassId
            var course = await firebaseClient
                .Child("Courses")
                .Child(cls.CourseId.ToString())
                .OnceSingleAsync<Course>();

            if (course != null)
            {
                if (course.ClassIds == null)
                    course.ClassIds = new List<int>();

                if (!course.ClassIds.Contains(cls.ClassId))
                {
                    course.ClassIds.Add(cls.ClassId);
                    await firebaseClient
                        .Child("Courses")
                        .Child(cls.CourseId.ToString())
                        .PutAsync(course);
                }
            }

            return Ok(cls);
        }


        // Update Class
        [HttpPut("{classId}")]
        public async Task<IActionResult> UpdateClassAsync(string classId, [FromBody] Class cls)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await firebaseClient
                .Child("Classes")
                .Child(classId)
                .PutAsync(cls);

            return Ok(cls);
        }

        // Delete Class
        [HttpDelete("{classId}")]
        public async Task<IActionResult> DeleteClassAsync(string classId)
        {
            try
            {
                // Retrieve class first
                var classToDelete = await firebaseClient
                    .Child("Classes")
                    .Child(classId)
                    .OnceSingleAsync<Class>();

                if (classToDelete == null)
                {
                    Console.WriteLine($"Class {classId} not found in Firebase.");
                    return NotFound($"Class {classId} not found.");
                }

                // Remove ClassId from Course.ClassIds
                var course = await firebaseClient
                    .Child("Courses")
                    .Child(classToDelete.CourseId.ToString())
                    .OnceSingleAsync<Course>();

                if (course != null && course.ClassIds != null)
                {
                    if (course.ClassIds.Contains(classToDelete.ClassId))
                    {
                        course.ClassIds.Remove(classToDelete.ClassId);
                        await firebaseClient
                            .Child("Courses")
                            .Child(course.CourseId.ToString())
                            .PutAsync(course);
                    }
                }

                // Now delete the class
                await firebaseClient
                    .Child("Classes")
                    .Child(classId)
                    .DeleteAsync();

                Console.WriteLine($"Class {classId} and its reference in Course {classToDelete.CourseId} deleted.");
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting class: " + ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // Get Class by ID
        [HttpGet("{classId}")]
        public async Task<ActionResult<Class>> GetClassAsync(string classId)
        {
            var cls = await firebaseClient
                .Child("Classes")
                .Child(classId)
                .OnceSingleAsync<Class>();

            if (cls == null) return NotFound();
            return Ok(cls);
        }

        // Get All Classes
        [HttpGet("all")]
        public async Task<ActionResult<List<Class>>> GetAllClassesAsync()
        {
            try
            {
                var classes = await GetAllClassesInternal();
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get classes: {ex.Message}");
            }
        }


        // Filter by CourseId or TeacherId
        [HttpGet("filter")]
        public async Task<ActionResult<List<Class>>> FilterByCourseOrTeacher([FromQuery] int? courseId, [FromQuery] int? teacherId)
        {
            var allClasses = await GetAllClassesInternal();

            var filtered = allClasses
                .Where(c =>
                    (!courseId.HasValue || c.CourseId == courseId.Value) &&
                    (!teacherId.HasValue || c.TeacherId == teacherId.Value))
                .ToList();

            return Ok(filtered);
        }

        // Filter by Date
        [HttpGet("date")]
        public async Task<ActionResult<List<Class>>> GetClassesByDate([FromQuery] DateTime date)
        {
            var allClasses = await GetAllClassesInternal();

            var classesOnDate = allClasses
                .Where(c => date.Date >= c.StartDate.Date && date.Date <= c.EndDate.Date)
                .ToList();

            return Ok(classesOnDate);
        }

        // Add Student
        [HttpPost("{classId}/students/{studentId}")]
        public async Task<IActionResult> AddStudentToClass(string classId, int studentId)
        {
            var cls = await firebaseClient
                .Child("Classes")
                .Child(classId)
                .OnceSingleAsync<Class>();

            if (cls == null)
                return NotFound("Class not found.");

            if (!cls.StudentIds.Contains(studentId))
            {
                cls.StudentIds.Add(studentId);
                await firebaseClient.Child("Classes").Child(classId).PutAsync(cls);
            }

            return Ok(cls);
        }

        // Remove Student
        [HttpDelete("{classId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudentFromClass(string classId, int studentId)
        {
            var cls = await firebaseClient
                .Child("Classes")
                .Child(classId)
                .OnceSingleAsync<Class>();

            if (cls == null)
                return NotFound("Class not found.");

            if (cls.StudentIds.Contains(studentId))
            {
                cls.StudentIds.Remove(studentId);
                await firebaseClient.Child("Classes").Child(classId).PutAsync(cls);
            }

            return Ok(cls);
        }

        // Internal method to get all classes as Dictionary
        private async Task<List<Class>> GetAllClassesInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Classes.json";

            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Class>>(json);
                if (dict != null) return dict.Values.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse as dictionary: " + ex.Message);
            }

            try
            {
                var list = JsonConvert.DeserializeObject<List<Class>>(json);
                if (list != null) return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse as list: " + ex.Message);
            }

            throw new Exception("Unable to parse class data from Firebase.");
        }
    }
}

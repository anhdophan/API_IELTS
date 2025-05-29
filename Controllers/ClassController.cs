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

            await firebaseClient
                .Child("Classes")
                .Child(cls.ClassId.ToString())
                .PutAsync(cls);

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
            await firebaseClient
                .Child("Classes")
                .Child(classId)
                .DeleteAsync();
            return Ok();
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
                return BadRequest($"Failed to parse classes from Firebase. Error: {ex.Message}");
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

    using (var httpClient = new HttpClient())
    {
        var json = await httpClient.GetStringAsync(url);

        try
        {
            // TH1: Dữ liệu là object: { "123": {...}, "456": {...} }
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Class>>(json);
            if (dict != null)
                return dict.Values.ToList();
        }
        catch { }

        try
        {
            // TH2: Dữ liệu là array: [ {...}, {...} ]
            var list = JsonConvert.DeserializeObject<List<Class>>(json);
            if (list != null)
                return list;
        }
        catch { }

        throw new Exception("Failed to parse data as Dictionary<string, Class> or List<Class>.");
    }
}

    }
}

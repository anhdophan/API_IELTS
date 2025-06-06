using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using api.Services;
using api.Models;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        private async Task LogAdminAction(string action, string performedBy, string description)
        {
            var log = new AdminLog
            {
                LogId = Guid.NewGuid().GetHashCode(),
                Action = action,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow,
                Description = description
            };

            await firebaseClient
                .Child("AdminLogs")
                .Child(log.LogId.ToString())
                .PutAsync(log);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourseAsync([FromBody] Course course)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await firebaseClient
                .Child("Courses")
                .Child(course.CourseId.ToString())
                .OnceSingleAsync<Course>();

            if (existing != null)
                return Conflict($"Course with ID {course.CourseId} already exists.");

            await firebaseClient
                .Child("Courses")
                .Child(course.CourseId.ToString())
                .PutAsync(course);

            await LogAdminAction("CreateCourse", "admin", $"Created CourseId={course.CourseId} - {course.Name}");
            return Ok(course);
        }

        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourseAsync(string courseId, [FromBody] Course course)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await firebaseClient
                .Child("Courses")
                .Child(courseId)
                .PutAsync(course);

            await LogAdminAction("UpdateCourse", "admin", $"Updated CourseId={courseId} - {course.Name}");
            return Ok(course);
        }

        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            var course = await firebaseClient
                .Child("Courses")
                .Child(courseId)
                .OnceSingleAsync<Course>();

            if (course == null)
                return NotFound("Course not found.");

            // Count classes using this course
            var classes = await firebaseClient.Child("Classes").OnceAsync<Class>();
            var affectedClasses = classes.Where(c => c.Object.CourseId.ToString() == courseId).Count();

            await firebaseClient
                .Child("Courses")
                .Child(courseId)
                .DeleteAsync();

            await LogAdminAction("DeleteCourse", "admin", $"Deleted CourseId={courseId}. Affected {affectedClasses} class(es)");
            return Ok();
        }

        [HttpGet("{courseId}")]
        public async Task<ActionResult<Course>> GetCourseAsync(string courseId)
        {
            var course = await firebaseClient
                .Child("Courses")
                .Child(courseId)
                .OnceSingleAsync<Course>();

            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Course>>> GetAllCoursesAsync()
        {
            try
            {
                var allCourses = await firebaseClient
                    .Child("Courses")
                    .OnceAsync<Course>();

                var list = allCourses
                    .Where(c => c.Object != null)
                    .Select(c => c.Object)
                    .ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                await LogAdminAction("GetAllCoursesError", "system", ex.Message);
                return BadRequest("Failed to parse courses from Firebase.");
            }
        }

        [HttpGet("date")]
        public async Task<ActionResult<List<Course>>> GetCoursesByDate([FromQuery] DateTime date)
        {
            var allCourses = await GetAllCoursesInternal();
            var filtered = allCourses
                .Where(c => date.Date >= c.StartDay.Date && date.Date <= c.EndDay.Date)
                .ToList();
            return Ok(filtered);
        }

        [HttpGet("cost")]
        public async Task<ActionResult<List<Course>>> FilterCoursesByCost([FromQuery] double? minCost, [FromQuery] double? maxCost)
        {
            var allCourses = await GetAllCoursesInternal();

            var filtered = allCourses
                .Where(c => (!minCost.HasValue || c.Cost >= minCost.Value) &&
                            (!maxCost.HasValue || c.Cost <= maxCost.Value))
                .ToList();

            return Ok(filtered);
        }

        private async Task<List<Course>> GetAllCoursesInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Courses.json";

            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Course>>(json);
                if (dict != null)
                    return dict.Values.Where(c => c != null).ToList(); // lọc null
            }
            catch { }

            try
            {
                var list = JsonConvert.DeserializeObject<List<Course>>(json);
                if (list != null)
                    return list.Where(c => c != null).ToList(); // lọc null ở đây
            }
            catch { }

            throw new Exception("Failed to parse data as Dictionary<string, Course> or List<Course>.");
        }

    }
}

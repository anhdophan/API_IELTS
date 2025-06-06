using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using api.Services;
using api.Models;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient;
        private readonly ILogger<CourseController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public CourseController(ILogger<CourseController> logger, IHttpClientFactory httpClientFactory)
        {
            firebaseClient = FirebaseService.Client;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

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
                var courses = await GetAllCoursesInternal();
                return Ok(courses);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error in GetAllCoursesAsync");
                return StatusCode(500, "Failed to retrieve courses. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAllCoursesAsync");
                return StatusCode(500, "An unexpected error occurred while retrieving courses.");
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
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Courses.json";
                var response = await client.GetAsync(url);
                
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json) || json == "null")
                {
                    _logger.LogWarning("No courses found in database");
                    return new List<Course>();
                }

                try
                {
                    // Try parsing as array first since we know the data is coming as an array
                    var courses = JsonConvert.DeserializeObject<List<Course>>(json, new JsonSerializerSettings 
                    { 
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    });

                    if (courses != null && courses.Any())
                    {
                        _logger.LogInformation($"Successfully retrieved {courses.Count} courses");
                        return courses.Where(c => c != null && c.CourseId >= 0).ToList();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse courses as array");
                    await LogAdminAction("ParseError", "system", $"Failed to parse course data: {ex.Message}");
                    throw new ApplicationException("Failed to parse course data", ex);
                }

                _logger.LogWarning("No valid courses found after parsing");
                return new List<Course>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while retrieving courses");
                await LogAdminAction("GetCoursesError", "system", $"HTTP request failed: {ex.Message}");
                throw new ApplicationException("Failed to retrieve courses from database", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAllCoursesInternal");
                await LogAdminAction("GetCoursesError", "system", $"Unexpected error: {ex.Message}");
                throw;
            }
        }

    }
}

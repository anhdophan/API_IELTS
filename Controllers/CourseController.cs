using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Services;
using api.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create Course
        [HttpPost]
        public async Task<IActionResult> CreateCourseAsync([FromBody] Course course)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await firebaseClient
                .Child("Courses")
                .Child(course.CourseId.ToString())
                .OnceSingleAsync<Course>();

            if (existing != null)
            {
                return Conflict($"Course with ID {course.CourseId} already exists.");
            }

            await firebaseClient
                .Child("Courses")
                .Child(course.CourseId.ToString())
                .PutAsync(course);

            return Ok(course);
        }

        // Update Course
        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourseAsync(string courseId, [FromBody] Course course)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await firebaseClient
                .Child("Courses")
                .Child(courseId)
                .PutAsync(course);

            return Ok(course);
        }

        // Delete Course
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            await firebaseClient
                .Child("Courses")
                .Child(courseId)
                .DeleteAsync();
            return Ok();
        }

        // Get Course by ID
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

        // Get All Courses (handle both Dictionary or List)
        [HttpGet("all")]
        public async Task<ActionResult<List<Course>>> GetAllCoursesAsync()
        {
            try
            {
                var courses = await GetAllCoursesInternal();
                return Ok(courses);
            }
            catch
            {
                return BadRequest("Failed to parse courses from Firebase.");
            }
        }

        // Filter Courses by Date (StartDay <= date <= EndDay)
        [HttpGet("date")]
        public async Task<ActionResult<List<Course>>> GetCoursesByDate([FromQuery] DateTime date)
        {
            var allCourses = await GetAllCoursesInternal();

            var filtered = allCourses
                .Where(c => date.Date >= c.StartDay.Date && date.Date <= c.EndDay.Date)
                .ToList();

            return Ok(filtered);
        }

        // Filter Courses by Cost range
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

        // Private helper to get all courses from Firebase, support Dictionary or List JSON
        private async Task<List<Course>> GetAllCoursesInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Courses.json";
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);

                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Course>>(json);
                    if (dict != null)
                        return dict.Values.ToList();
                }
                catch { }

                try
                {
                    var list = JsonConvert.DeserializeObject<List<Course>>(json);
                    if (list != null)
                        return list;
                }
                catch { }

                throw new Exception("Failed to parse data as Dictionary<string, Course> or List<Course>.");
            }
        }
    }
}

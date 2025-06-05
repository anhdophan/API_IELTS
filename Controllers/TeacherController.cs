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

        // Create
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

        // Update
        [HttpPut("{teacherId}")]
        public async Task<IActionResult> UpdateTeacherAsync(string teacherId, [FromBody] Teacher teacher)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .PutAsync(teacher);

            return Ok(teacher);
        }

        // Delete
        [HttpDelete("{teacherId}")]
        public async Task<IActionResult> DeleteTeacherAsync(string teacherId)
        {
            await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .DeleteAsync();
            return Ok();
        }

        // Get by ID
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

        // Get All (Dictionary or List support)
        [HttpGet("all")]
        public async Task<ActionResult<List<Teacher>>> GetAllTeachersAsync()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Teachers.json";

            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Teacher>>(json);
                if (dict != null) return Ok(dict.Values.ToList());
            }
            catch { }

            try
            {
                var list = JsonConvert.DeserializeObject<List<Teacher>>(json);
                if (list != null) return Ok(list);
            }
            catch { }

            return BadRequest("Unable to parse Teachers from Firebase.");
        }
    }
}

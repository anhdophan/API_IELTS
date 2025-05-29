using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Services;
using api.Models;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        [HttpPost]
        public async Task<IActionResult> CreateTeacherAsync([FromBody] Teacher teacher)
        {
            await firebaseClient
                .Child("Teachers")
                .Child(teacher.TeacherId.ToString())
                .PutAsync(teacher);
            return Ok();
        }

        [HttpPut("{teacherId}")]
        public async Task<IActionResult> UpdateTeacherAsync(string teacherId, [FromBody] Teacher teacher)
        {
            await firebaseClient
                .Child("Teachers")
                .Child(teacherId)
                .PutAsync(teacher);
            return Ok();
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

        [HttpGet]
        public async Task<ActionResult<List<Teacher>>> GetAllTeachersAsync()
        {
            var teachers = await firebaseClient
                .Child("Teachers")
                .OnceAsync<Teacher>();

            var teacherList = new List<Teacher>();
            foreach (var t in teachers)
            {
                teacherList.Add(t.Object);
            }
            return Ok(teacherList);
        }
    }
}
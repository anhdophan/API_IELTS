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
    public class StudySessionController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create StudySession
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] StudySession session)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Thêm ngày tạo nếu chưa có
            if (session.DateCreated == default)
                session.DateCreated = DateTime.UtcNow;

            var newRef = await firebaseClient
                .Child("StudySessions")
                .PostAsync(session);

            session.Id = newRef.Key.GetHashCode(); // Lưu Id dựa trên Firebase key hash để có thể truy xuất
            await firebaseClient
                .Child("StudySessions")
                .Child(newRef.Key)
                .PutAsync(session);

            return Ok(session);
        }

        // Update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] StudySession session)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .PutAsync(session);

            return Ok(session);
        }

        // Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .DeleteAsync();

            return Ok();
        }

        // Get one
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var session = await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .OnceSingleAsync<StudySession>();

            if (session == null) return NotFound();
            return Ok(session);
        }

        // Get all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var sessions = await firebaseClient
                .Child("StudySessions")
                .OnceAsync<StudySession>();

            var list = sessions.Select(s => s.Object).ToList();
            return Ok(list);
        }

        // Filter by ClassID
        [HttpGet("class")]
        public async Task<IActionResult> GetByClassIdAsync([FromQuery] int classId)
        {
            var sessions = await firebaseClient
                .Child("StudySessions")
                .OnceAsync<StudySession>();

            var filtered = sessions.Select(s => s.Object)
                .Where(s => s.ClassID == classId)
                .ToList();

            return Ok(filtered);
        }

        // Filter by DateCreated (optional: from - to)
        [HttpGet("date")]
        public async Task<IActionResult> FilterByDateAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var sessions = await firebaseClient
                .Child("StudySessions")
                .OnceAsync<StudySession>();

            var filtered = sessions.Select(s => s.Object).AsQueryable();

            if (from.HasValue)
                filtered = filtered.Where(s => s.DateCreated >= from.Value);

            if (to.HasValue)
                filtered = filtered.Where(s => s.DateCreated <= to.Value);

            return Ok(filtered.ToList());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Models;
using api.Services;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudySessionController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient;
        private readonly ILogger<StudySessionController> _logger;

        public StudySessionController(ILogger<StudySessionController> logger)
        {
            firebaseClient = FirebaseService.Client;
            _logger = logger;
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
        public async Task<IActionResult> CreateStudySessionAsync([FromBody] StudySession session)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (session.DateCreated == default)
                session.DateCreated = DateTime.UtcNow;

            // Sinh ID nếu chưa có
            if (session.Id == 0)
                session.Id = Guid.NewGuid().GetHashCode();

            var existing = await firebaseClient
                .Child("StudySessions")
                .Child(session.Id.ToString())
                .OnceSingleAsync<StudySession>();

            if (existing != null)
                return Conflict($"StudySession with ID {session.Id} already exists.");

            await firebaseClient
                .Child("StudySessions")
                .Child(session.Id.ToString())
                .PutAsync(session);

            await LogAdminAction("CreateStudySession", "admin", $"Created StudySessionId={session.Id} for ClassId={session.ClassID} - {session.Material}");
            return Ok(session);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudySessionAsync(string id, [FromBody] StudySession session)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .PutAsync(session);

            await LogAdminAction("UpdateStudySession", "admin", $"Updated StudySessionId={id} for ClassId={session.ClassID} - {session.Material}");
            return Ok(session);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudySessionAsync(string id)
        {
            var session = await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .OnceSingleAsync<StudySession>();

            if (session == null)
                return NotFound("StudySession not found.");

            await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .DeleteAsync();

            await LogAdminAction("DeleteStudySession", "admin", $"Deleted StudySessionId={id} for ClassId={session.ClassID}");
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudySession>> GetStudySessionByIdAsync(string id)
        {
            var session = await firebaseClient
                .Child("StudySessions")
                .Child(id)
                .OnceSingleAsync<StudySession>();

            if (session == null) return NotFound();
            return Ok(session);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<StudySession>>> GetAllStudySessionsAsync()
        {
            var sessions = await firebaseClient
                .Child("StudySessions")
                .OnceAsync<StudySession>();

            var list = sessions.Select(s => s.Object).ToList();
            return Ok(list);
        }

        [HttpGet("class")]
        public async Task<ActionResult<List<StudySession>>> GetStudySessionsByClassIdAsync([FromQuery] int classId)
        {
            var sessions = await firebaseClient
                .Child("StudySessions")
                .OnceAsync<StudySession>();

            var filtered = sessions.Select(s => s.Object)
                .Where(s => s.ClassID == classId)
                .ToList();

            return Ok(filtered);
        }

        [HttpGet("date")]
        public async Task<ActionResult<List<StudySession>>> FilterByDateAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
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

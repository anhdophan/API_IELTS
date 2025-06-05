// Controllers/AdminLogController.cs
using Microsoft.AspNetCore.Mvc;
using api.Models;
using Firebase.Database;
using Firebase.Database.Query;
using api.Services;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminLogController : ControllerBase
    {
        private readonly FirebaseClient _client = FirebaseService.Client;

        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] AdminLog log)
        {
            log.LogId = Guid.NewGuid().GetHashCode();
            log.Timestamp = DateTime.UtcNow;

            await _client.Child("AdminLogs")
                         .Child(log.LogId.ToString())
                         .PutAsync(log);

            return Ok(log);
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminLog>>> GetAllLogs()
        {
            var logs = await _client.Child("AdminLogs").OnceAsync<AdminLog>();
            return Ok(logs.Select(l => l.Object).OrderByDescending(l => l.Timestamp).ToList());
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using api.Services;
using Newtonsoft.Json;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FcmController : ControllerBase
    {
      private readonly FirebaseClient firebaseClient = FirebaseService.Client;


       [HttpPost("register-token")]
public async Task<IActionResult> RegisterFcmToken([FromBody] TokenRequest request)
{
    if (string.IsNullOrWhiteSpace(request.StudentId) || string.IsNullOrWhiteSpace(request.FcmToken))
    {
        return BadRequest(new { error = "Missing StudentId or FcmToken" });
    }

    Console.WriteLine($"✅ Saving token: StudentId={request.StudentId}, Token={request.FcmToken}");

    try
    {
      await firebaseClient
  .Child("Tokens")
  .Child(request.StudentId.ToString())
  .Child("fcmToken")
  .PutAsync(JsonConvert.SerializeObject(request.FcmToken));

        return Ok(new { message = "Token saved successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Firebase error: {ex.Message}");
        return StatusCode(500, new { error = ex.Message });
    }
}

    }

    public class TokenRequest
    {
        public string StudentId { get; set; }
        public string FcmToken { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FcmController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient;

        public FcmController()
        {
            // Khởi tạo FirebaseClient nếu chưa có DI
            firebaseClient = new FirebaseClient("https://<your-project-id>.firebaseio.com/");
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterFcmToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.StudentId) || string.IsNullOrEmpty(request.FcmToken))
                return BadRequest("Missing StudentId or FcmToken");

            await firebaseClient
                .Child("Tokens")
                .Child(request.StudentId)
                .Child("fcmToken")
                .PutAsync(request.FcmToken);

            return Ok(new { message = "Token saved successfully" });
        }
    }

    public class TokenRequest
    {
        public string StudentId { get; set; }
        public string FcmToken { get; set; }
    }
}

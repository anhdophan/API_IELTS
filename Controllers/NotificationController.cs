using Microsoft.AspNetCore.Mvc;
using api.Services;

namespace api.Controllers
{
          [Route("api/[controller]")]
          [ApiController]
          public class NotificationController : ControllerBase
          {
                    private readonly FirebaseMessagingService _fcmService;

                    public NotificationController(FirebaseMessagingService fcmService)
                    {
                              _fcmService = fcmService;
                    }

                    [HttpPost("send")]
                    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
                    {
                              var result = await _fcmService.SendNotificationToDeviceAsync(
                                  request.FcmToken, request.Title, request.Body);

                              return Ok(new { messageId = result });
                    }
          }


          public class NotificationRequest
          {
                    public string FcmToken { get; set; }
                    public string Title { get; set; }
                    public string Body { get; set; }
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

public class TokenRequest
{
    public string StudentId { get; set; }
    public string FcmToken { get; set; }
}

}

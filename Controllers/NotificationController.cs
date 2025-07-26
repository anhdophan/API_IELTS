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
          
          
}

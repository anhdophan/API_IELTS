using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;


namespace api.Services
{
          public class FirebaseMessagingService
          {
                    private static bool _initialized = false;

                    public FirebaseMessagingService()
                    {
                              if (!_initialized)
                              {
                                        FirebaseApp.Create(new AppOptions()
                                        {
                                                  Credential = GoogleCredential.FromFile("Configs/firebase-adminsdk.json"),
                                        });
                                        _initialized = true;
                              }
                    }

                    public async Task<string> SendNotificationToDeviceAsync(string fcmToken, string title, string body)
                    {
                              var message = new Message()
                              {
                                        Token = fcmToken,
                                        Notification = new FirebaseAdmin.Messaging.Notification
                                        {
                                        Title = title,
                                        Body = body
                                        },
                                        Android = new AndroidConfig
                                        {
                                                  Priority = Priority.High,
                                                  Notification = new AndroidNotification
                                                  {
                                                            Sound = "default"
                                                  }
                                        },
                                        Apns = new ApnsConfig
                                        {
                                                  Aps = new Aps
                                                  {
                                                            Sound = "default"
                                                  }
                                        },
                                        Data = new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                { "type", "chat" }
            }
                              };

                              try
                              {
                                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                                        Console.WriteLine("Gửi thành công: " + response);
                                        return response;
                              }
                              catch (Exception ex)
                              {
                                        Console.WriteLine("Lỗi gửi FCM: " + ex.Message);
                                        return null;
                              }
                    }
          }
}
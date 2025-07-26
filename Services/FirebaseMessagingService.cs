using Firebase.Database.Query;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace api.Services
{
    public class FirebaseMessagingService
    {
        private static bool _initialized = false;

        public FirebaseMessagingService()
        {
            if (!_initialized)
            {
                var jsonString = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
                if (string.IsNullOrEmpty(jsonString))
                {
                    throw new Exception("FIREBASE_CREDENTIALS_JSON environment variable not set.");
                }

                var tempPath = Path.Combine(Path.GetTempPath(), "firebase-adminsdk.json");
                File.WriteAllText(tempPath, jsonString);

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(tempPath)
                });

                _initialized = true;
            }
        }
          public async Task<string> SendNotificationToStudentAsync(string studentId, string title, string body)
          {
          var tokenSnapshot = await FirebaseService.Client
          .Child("Tokens")
          .Child(studentId)
          .Child("fcmToken")
          .OnceSingleAsync<string>();

          if (string.IsNullOrEmpty(tokenSnapshot))
          throw new Exception("FCM Token not found");

          return await SendNotificationToDeviceAsync(tokenSnapshot, title, body);
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
                Data = new Dictionary<string, string>
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

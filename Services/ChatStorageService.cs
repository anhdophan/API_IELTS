using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;
using api.Services;
using System.Text.RegularExpressions;
using System.Text.Json;
using api.Models;

namespace api.Services
{
    public static class ChatStorageService
    {
        private static FirebaseClient _client => FirebaseService.Client;

        private static string GetPrivateChatKey(string userA, string userB)
        {
            var users = new[] { userA, userB };
            Array.Sort(users);
            return $"{users[0]}_{users[1]}";
        }

       public static async Task SavePrivateMessageAsync(string senderId, string receiverId, string message)
        {
            var chatKey = GetPrivateChatKey(senderId, receiverId);
            var timestamp = DateTime.UtcNow;

            // 1. Lưu vào Firebase
            await _client
                .Child("chats")
                .Child("private")
                .Child(chatKey)
                .PostAsync(new
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = message,
                    Timestamp = timestamp
                });

            // 2. Gửi FCM
            var fcmService = new FirebaseMessagingService();
            await fcmService.SendNotificationToStudentAsync(receiverId, 
                "Tin nhắn mới", 
                $"Bạn có tin nhắn mới từ ID {senderId}");
        }


        public static async Task SaveGroupMessageAsync(string senderId, int classId, string message)
    {
        var timestamp = DateTime.UtcNow;

        await _client
            .Child("chats")
            .Child("group")
            .Child($"class_{classId}")
            .PostAsync(new
            {
                SenderId = senderId,
                Message = message,
                Timestamp = timestamp
            });

        // Gửi thông báo cho tất cả học sinh trong lớp
        var client = new HttpClient();
        var response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/students/class?className={classId}");
        var json = await response.Content.ReadAsStringAsync();
        var students = JsonSerializer.Deserialize<List<Student>>(json);

        var fcmService = new FirebaseMessagingService();
        foreach (var student in students.Where(s => s.StudentId.ToString() != senderId)) {
            await fcmService.SendNotificationToStudentAsync(student.StudentId.ToString(), 
                "Tin nhắn lớp mới", 
                $"{senderId} vừa gửi một tin nhắn trong lớp");
        }
    }

    }
}

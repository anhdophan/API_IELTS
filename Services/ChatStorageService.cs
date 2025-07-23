using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace api.Hubs
{
    public class ChatHub : Hub
    {
        // Lưu thông tin kết nối: userId -> ConnectionId
        private static ConcurrentDictionary<string, string> userConnections = new();

        // Lưu thông tin class mà student thuộc về: userId -> List<classId>
        private static ConcurrentDictionary<string, List<int>> studentClasses = new();

        // Khi client kết nối, gửi query ?userId=...&classIds=1,2,3
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"].ToString();
            var classIdsRaw = httpContext.Request.Query["classIds"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                userConnections[userId] = Context.ConnectionId;

                if (!string.IsNullOrEmpty(classIdsRaw))
                {
                    var classes = classIdsRaw.Split(',')
                                             .Select(c => int.TryParse(c, out var id) ? id : -1)
                                             .Where(id => id > 0)
                                             .ToList();
                    studentClasses[userId] = classes;
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var user = userConnections.FirstOrDefault(kvp => kvp.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(user))
            {
                userConnections.TryRemove(user, out _);
                studentClasses.TryRemove(user, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Gửi tin nhắn giữa 2 student nếu họ thuộc cùng 1 class
        /// </summary>
        public async Task SendPrivateMessage(string senderId, string receiverId, string message)
        {
            if (!userConnections.ContainsKey(receiverId) || !studentClasses.ContainsKey(senderId) || !studentClasses.ContainsKey(receiverId))
            {
                return;
            }

            // Kiểm tra có lớp chung không
            var senderClasses = studentClasses[senderId];
            var receiverClasses = studentClasses[receiverId];

            var hasSharedClass = senderClasses.Intersect(receiverClasses).Any();

            if (hasSharedClass)
            {
                var connectionId = userConnections[receiverId];
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId, message);
            }
        }
    }
}

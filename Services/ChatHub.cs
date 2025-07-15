using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace api.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> userConnections = new();
        private static ConcurrentDictionary<string, List<int>> studentClasses = new();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"].ToString();
            var classIdsRaw = httpContext.Request.Query["classIds"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                userConnections[userId] = Context.ConnectionId;

                // Chỉ thêm class nếu không phải admin
                if (userId != "admin" && !string.IsNullOrEmpty(classIdsRaw))
                {
                    var classes = classIdsRaw.Split(',')
                                             .Select(c => int.TryParse(c, out var id) ? id : -1)
                                             .Where(id => id != -1)
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

        public async Task SendPrivateMessage(string senderId, string receiverId, string message)
        {
            // Cho phép admin chat với bất kỳ ai và ngược lại
            if (senderId == "admin" || receiverId == "admin")
            {
                if (userConnections.TryGetValue(receiverId, out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId, message);
                }
                return;
            }

            // Kiểm tra 2 học sinh có chung lớp
            if (!studentClasses.ContainsKey(senderId) || !studentClasses.ContainsKey(receiverId))
                return;

            var senderClasses = studentClasses[senderId];
            var receiverClasses = studentClasses[receiverId];
            var hasSharedClass = senderClasses.Intersect(receiverClasses).Any();

            if (hasSharedClass && userConnections.TryGetValue(receiverId, out var connId))
            {
                await Clients.Client(connId).SendAsync("ReceiveMessage", senderId, message);
            }
        }
    }
}

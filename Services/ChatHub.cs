using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using api.Services;

namespace api.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, List<string>> userConnections = new();
        private static readonly ConcurrentDictionary<string, List<int>> studentClasses = new();
        private static readonly HashSet<string> onlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"].ToString();
            var classIdsRaw = httpContext.Request.Query["classIds"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                lock (userConnections)
                {
                    if (!userConnections.ContainsKey(userId))
                        userConnections[userId] = new List<string>();

                    userConnections[userId].Add(Context.ConnectionId);
                }

                lock (onlineUsers)
                {
                    onlineUsers.Add(userId);
                }

                await Clients.All.SendAsync("UserOnline", userId);

                // Thêm user vào các nhóm lớp
                if (userId != "admin" && !string.IsNullOrEmpty(classIdsRaw))
                {
                    var classIds = classIdsRaw.Split(',')
                        .Select(id => int.TryParse(id, out var parsed) ? parsed : -1)
                        .Where(id => id != -1)
                        .ToList();

                    studentClasses[userId] = classIds;

                    foreach (var classId in classIds)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"class_{classId}");
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
{
    string disconnectedUser = null;
    bool shouldNotifyOffline = false;

    lock (userConnections)
    {
        disconnectedUser = userConnections
            .FirstOrDefault(kvp => kvp.Value.Contains(Context.ConnectionId)).Key;

        if (!string.IsNullOrEmpty(disconnectedUser))
        {
            userConnections[disconnectedUser].Remove(Context.ConnectionId);

            if (userConnections[disconnectedUser].Count == 0)
            {
                userConnections.TryRemove(disconnectedUser, out _);
                lock (onlineUsers)
                {
                    onlineUsers.Remove(disconnectedUser);
                }
                studentClasses.TryRemove(disconnectedUser, out _);
                shouldNotifyOffline = true;
            }
        }
    }

    if (shouldNotifyOffline && !string.IsNullOrEmpty(disconnectedUser))
    {
        await Clients.All.SendAsync("UserOffline", disconnectedUser);
    }

    await base.OnDisconnectedAsync(exception);
}


   public async Task SendPrivateMessage(string senderId, string receiverId, string message)
{
    await ChatStorageService.SavePrivateMessageAsync(senderId, receiverId, message);

    if (senderId == "admin" || receiverId == "admin")
    {
        if (userConnections.TryGetValue(receiverId, out var connections))
        {
            foreach (var connId in connections)
            {
                await Clients.Client(connId).SendAsync("ReceiveMessage", senderId, message);
            }
        }
        return;
    }

    if (studentClasses.TryGetValue(senderId, out var senderClasses) &&
        studentClasses.TryGetValue(receiverId, out var receiverClasses))
    {
        var shared = senderClasses.Intersect(receiverClasses).Any();
        if (shared && userConnections.TryGetValue(receiverId, out var receiverConnections))
        {
            foreach (var connId in receiverConnections)
            {
                await Clients.Client(connId).SendAsync("ReceiveMessage", senderId, message);
            }
        }
    }
}

public async Task SendGroupMessage(string senderId, int classId, string message)
{
    await ChatStorageService.SaveGroupMessageAsync(senderId, classId, message);
    await Clients.Group($"class_{classId}").SendAsync("ReceiveGroupMessage", senderId, message);
}


        public List<string> GetOnlineUsers()
        {
            lock (onlineUsers)
            {
                return onlineUsers.ToList();
            }
        }
    }
}

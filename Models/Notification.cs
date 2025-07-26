using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Models;
using api.Services;
using Microsoft.Extensions.Logging;

namespace api.Models
{
    public class Notification
    {
        public string NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false;

        // Optional: dùng để phân loại về sau
        public string Type { get; set; } = "exam"; // hoặc "chat", "announcement",...
    }

}
using System;
using System.Collections.Generic;

namespace api.Models
{
    public class ChatMessage
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; } // optional for group
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

}

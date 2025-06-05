// Models/AdminLog.cs
using System;

namespace api.Models
{
    public class AdminLog
    {
        public int LogId { get; set; }
        public string Action { get; set; }       // e.g., "DeleteCourse", "EditClass"
        public string PerformedBy { get; set; }  // e.g., admin username
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }  // e.g., "Deleted CourseId=12, affected 3 classes"
    }
}

using System;

namespace api.Models
{
    public class StudySession
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Material { get; set; } // e.g., "Chapter 1", "Exam", etc.
        public bool IsExam { get; set; }
    }
}
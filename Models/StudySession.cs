using System;

namespace api.Models
{
    public class StudySession
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; } // Start time of the study session
        public int ClassID { get; set; } // Foreign key to Class
        public string Material { get; set; } // e.g., "Chapter 1", "Exam", etc.
        public string Detail { get; set; } 
    
    }
}
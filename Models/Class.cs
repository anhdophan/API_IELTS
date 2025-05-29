using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> StudentIds { get; set; } = new List<int>();
        public List<string> Schedule { get; set; } = new List<string>(); // Use string for day names: "Monday", "Tuesday", etc.
    }
}
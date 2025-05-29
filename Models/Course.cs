using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public double Cost { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public string DescriptionShort { get; set; }
        public string DescriptionLong { get; set; }
        public List<string> Images { get; set; } // Store multiple image URLs or paths
    }
}
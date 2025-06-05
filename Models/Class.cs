using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        public int TeacherId { get; set; } = -1;  // -1 means "Chưa có giảng viên"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> StudentIds { get; set; } = new();
        public List<ClassSchedule> Schedule { get; set; } = new();
    }
}

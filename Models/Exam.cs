using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Name { get; set; }
        public DateTime ExamDate { get; set; }
        public int CourseId { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
    }
}
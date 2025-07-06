using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public int CourseId { get; set; }
        public DateTime ExamDate { get; set; }
        public List<ExamQuestion> Questions { get; set; }
        public string CreatedById { get; set; } // Thêm dòng này
    }

    public class ExamQuestion
    {
        public int QuestionId { get; set; }
        public double Score { get; set; } // VD: 0.25, 1.0, ...
    }
}
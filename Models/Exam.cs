using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public int IdClass { get; set; } // Đổi từ CourseId sang IdClass
        public DateTime ExamDate { get; set; }
        public List<ExamQuestion> Questions { get; set; }
        public string CreatedById { get; set; }
    }

    public class ExamQuestion
    {
        public int QuestionId { get; set; }
        public double Score { get; set; }
    }
}
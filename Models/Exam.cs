using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public int IdClass { get; set; }
        public DateTime ExamDate { get; set; }
        public List<ExamQuestion> Questions { get; set; }
        public string CreatedById { get; set; }

        public int DurationMinutes { get; set; }      // Thời gian làm bài (phút)
        public DateTime StartTime { get; set; }       // Thời điểm bắt đầu mở bài thi
        public DateTime EndTime { get; set; }         // Thời điểm kết thúc, khóa bài thi
    }

    public class ExamQuestion
    {
        public int QuestionId { get; set; }
        public double Score { get; set; }
    }
}
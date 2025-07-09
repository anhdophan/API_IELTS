using System;
using System.Collections.Generic;

namespace api.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public double Score { get; set; }
        public double TotalScore { get; set; } // Tổng điểm tối đa của bài thi
        public string Remark { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<string> Answers { get; set; } = new List<string>(); // Đáp án của Student (nếu cần)
        public int DurationSeconds { get; set; } // Thời gian làm bài thực tế (nếu cần)
    }
}

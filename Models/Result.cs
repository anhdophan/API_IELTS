using System;

namespace api.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public double Score { get; set; }
        public string Remark { get; set; }
    }
}
using System.Collections.Generic;

namespace api.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public bool IsMultipleChoice { get; set; }
        public List<string> Choices { get; set; } = new List<string>(); // luôn khởi tạo rỗng
        public int? CorrectAnswerIndex { get; set; } = new int?(); // khởi tạo null
        public string? CorrectInputAnswer { get; set; } = null; // có thể null nếu không phải dạng input

        public double Level { get; set; }
        public string CreatedById { get; set; }
    }
}
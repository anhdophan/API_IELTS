using System.Collections.Generic;

namespace api.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public List<string> Choices { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string CorrectInputAnswer { get; set; }
        public bool IsMultipleChoice { get; set; }
        public List<double> Levels { get; set; } // VD: [4.0, 4.5, 5.0]
    }
}
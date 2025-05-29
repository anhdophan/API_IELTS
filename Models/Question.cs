using System.Collections.Generic;

namespace api.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public List<string> Answers { get; set; } // Up to 4 answers for multiple choice
        public int? CorrectAnswerIndex { get; set; } // For multiple choice, 0-3
        public string CorrectInputAnswer { get; set; } // For input answer
        public bool IsMultipleChoice { get; set; }
    }
}
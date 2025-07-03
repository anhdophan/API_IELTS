using System.Collections.Generic;

namespace api.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public bool IsMultipleChoice { get; set; }
        public List<string> Choices { get; set; }
        public int? CorrectAnswerIndex { get; set; }
        public string CorrectInputAnswer { get; set; }
        public double Level { get; set; }
        public string CreatedById { get; set; }
    }
}
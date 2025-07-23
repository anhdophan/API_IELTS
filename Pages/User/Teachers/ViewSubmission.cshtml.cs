using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace api.Pages.User.Teachers
{
    public class ViewSubmissionModel : PageModel
    {
        public Result Result { get; set; } = new();
        public Student Student { get; set; } = new();
        public List<SubmissionAnswerViewModel> SubmissionAnswers { get; set; } = new();

        public double ScoreCorrect => SubmissionAnswers.Sum(a => a.Score);
        public double ScoreIncorrect => (Result.TotalScore - ScoreCorrect);

      public class SubmissionAnswerViewModel
{
    public int QuestionId { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;  // Giữ lại nếu là tự luận
    public string CorrectAnswer { get; set; } = string.Empty;
    public string StudentAnswer { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsMultipleChoice { get; set; }
}


        public async Task<IActionResult> OnGetAsync(int resultId)
        {
            using var httpClient = new HttpClient();

            // Lấy kết quả
            var resultStr = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Result/{resultId}");
            Result = JsonConvert.DeserializeObject<Result>(resultStr) ?? new Result();

            // Lấy sinh viên
            var studentStr = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{Result.StudentId}");
            Student = JsonConvert.DeserializeObject<Student>(studentStr) ?? new Student();

            // Lấy bài thi để biết thứ tự và điểm từng câu
            var examStr = await httpClient.GetStringAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{Result.ExamId}");
            var exam = JsonConvert.DeserializeObject<Exam>(examStr) ?? new Exam();

            // Lấy chi tiết câu hỏi cần thiết
            var questionsStr = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Question/all");
            var allQuestions = JsonConvert.DeserializeObject<List<Question>>(questionsStr) ?? new List<Question>();

            // Ghép câu trả lời theo thứ tự exam.Questions
          for (int idx = 0; idx < exam.Questions.Count; idx++)
{
    var eq = exam.Questions[idx];
    string studentAns = idx < Result.Answers.Count ? Result.Answers[idx] : string.Empty;
    var question = allQuestions.FirstOrDefault(q => q.QuestionId == eq.QuestionId);
    if (question == null) continue;

    var vm = new SubmissionAnswerViewModel
    {
        QuestionId = eq.QuestionId,
        QuestionContent = question.Content,
        IsMultipleChoice = question.IsMultipleChoice,
        Score = eq.Score
    };

    if (question.IsMultipleChoice)
    {
        int studentIndex = int.TryParse(studentAns, out var i) ? i : -1;
        int correctIndex = question.CorrectAnswerIndex ?? -1;

        string studentChoice = (studentIndex >= 0 && studentIndex < question.Choices.Count)
            ? question.Choices[studentIndex] : "(không hợp lệ)";
        string correctChoice = (correctIndex >= 0 && correctIndex < question.Choices.Count)
            ? question.Choices[correctIndex] : "(không rõ)";

        vm.StudentAnswer = studentChoice;
        vm.CorrectAnswer = correctChoice;
        vm.IsCorrect = studentIndex == correctIndex;
        vm.Score = vm.IsCorrect ? eq.Score : 0;
    }
    else
    {
        vm.StudentAnswer = studentAns;
        vm.CorrectAnswer = question.CorrectInputAnswer ?? "";
        vm.IsCorrect = string.Equals(studentAns?.Trim(), vm.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        vm.Score = vm.IsCorrect ? eq.Score : 0;
    }

    SubmissionAnswers.Add(vm);
}


            return Page();
        }
    }
}
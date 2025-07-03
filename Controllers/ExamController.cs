using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Services;
using api.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create Exam
        [HttpPost]
        public async Task<IActionResult> CreateExamAsync([FromBody] Exam exam)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (exam.Questions == null || exam.Questions.Count == 0)
                return BadRequest("Exam must contain at least one question.");

            // Kiểm tra từng câu hỏi có tồn tại không (nếu muốn)
            foreach (var eq in exam.Questions)
            {
                var q = await firebaseClient
                    .Child("Questions")
                    .Child(eq.QuestionId.ToString())
                    .OnceSingleAsync<Question>();
                if (q == null)
                    return BadRequest($"Question {eq.QuestionId} does not exist.");
            }

            var existing = await firebaseClient
                .Child("Exams")
                .Child(exam.ExamId.ToString())
                .OnceSingleAsync<Exam>();

            if (existing != null)
                return Conflict($"Exam with ID {exam.ExamId} already exists.");

            await firebaseClient
                .Child("Exams")
                .Child(exam.ExamId.ToString())
                .PutAsync(exam);

            return Ok(exam);
        }


        // Update Exam
        [HttpPut("{examId}")]
        public async Task<IActionResult> UpdateExamAsync(string examId, [FromBody] Exam exam)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await firebaseClient
                .Child("Exams")
                .Child(examId)
                .PutAsync(exam);

            return Ok(exam);
        }

        // Delete Exam
        [HttpDelete("{examId}")]
        public async Task<IActionResult> DeleteExamAsync(string examId)
        {
            await firebaseClient
                .Child("Exams")
                .Child(examId)
                .DeleteAsync();
            return Ok();
        }

        // Get Exam by Id
        [HttpGet("{examId}")]
        public async Task<ActionResult<Exam>> GetExamAsync(string examId)
        {
            var exam = await firebaseClient
                .Child("Exams")
                .Child(examId)
                .OnceSingleAsync<Exam>();

            if (exam == null) return NotFound();
            return Ok(exam);
        }

        // Get all Exams (support JSON Dictionary or List)
        [HttpGet("all")]
        public async Task<ActionResult<List<Exam>>> GetAllExamsAsync()
        {
            try
            {
                var exams = await GetAllExamsInternal();
                return Ok(exams);
            }
            catch
            {
                return BadRequest("Failed to parse exams from Firebase.");
            }
        }

        // Filter Exams by CourseId
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<List<Exam>>> GetExamsByCourse(int courseId)
        {
            var exams = await GetAllExamsInternal();
            var filtered = exams.Where(e => e.CourseId == courseId).ToList();
            return Ok(filtered);
        }

        // Filter Exams by ExamDate (on exact date)
        [HttpGet("date")]
        public async Task<ActionResult<List<Exam>>> GetExamsByDate([FromQuery] DateTime date)
        {
            var exams = await GetAllExamsInternal();
            var filtered = exams.Where(e => e.ExamDate.Date == date.Date).ToList();
            return Ok(filtered);
        }

        // Submit Exam Request DTO
        public class SubmitExamRequest
        {
            public int StudentId { get; set; }
            public int ExamId { get; set; }
            public List<string> Answers { get; set; } // Index or input text answers
        }

        [HttpPost("{examId}/submit")]
        public async Task<IActionResult> SubmitExamAsync(int examId, [FromBody] SubmitExamRequest request)
        {
            if (examId != request.ExamId)
                return BadRequest("ExamId in route and body must match.");
            var exam = await firebaseClient
                .Child("Exams")
                .Child(examId.ToString())
                .OnceSingleAsync<Exam>();

            if (exam == null)
                return NotFound("Exam not found");

            if (exam.Questions == null || exam.Questions.Count == 0)
                return BadRequest("Exam has no questions.");

            double score = 0;
            double totalScore = exam.Questions.Sum(q => q.Score);

            for (int i = 0; i < exam.Questions.Count; i++)
            {
                if (i >= request.Answers.Count) break;

                var eq = exam.Questions[i];
                var q = await firebaseClient
                    .Child("Questions")
                    .Child(eq.QuestionId.ToString())
                    .OnceSingleAsync<Question>();

                var userAnswer = request.Answers[i];

                bool correct = false;
                if (q.IsMultipleChoice)
                {
                    if (int.TryParse(userAnswer, out int idx) && q.CorrectAnswerIndex == idx)
                        correct = true;
                }
                else
                {
                    if (string.Equals(q.CorrectInputAnswer?.Trim(), userAnswer?.Trim(), StringComparison.OrdinalIgnoreCase))
                        correct = true;
                }

                if (correct)
                    score += eq.Score;
            }

            var result = new Result
            {
                ResultId = int.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(5, 8)),
                StudentId = request.StudentId,
                ExamId = examId,
                Score = score,
                Remark = $"You got {score} out of {totalScore}",
                Timestamp = DateTime.UtcNow
            };

            await firebaseClient
                .Child("Results")
                .Child(result.ResultId.ToString())
                .PutAsync(result);

            return Ok(result);
        }

        // Private helper: get all exams from Firebase, support Dictionary or List JSON
        private async Task<List<Exam>> GetAllExamsInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Exams.json";
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);

                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Exam>>(json);
                    if (dict != null)
                        return dict.Values.ToList();
                }
                catch { }

                try
                {
                    var list = JsonConvert.DeserializeObject<List<Exam>>(json);
                    if (list != null)
                        return list;
                }
                catch { }

                throw new Exception("Failed to parse data as Dictionary<string, Exam> or List<Exam>.");
            }
        }
    }
}

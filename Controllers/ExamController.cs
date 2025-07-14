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

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); // Linux
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
            }
        }

        // Create Exam
        [HttpPost]
        public async Task<IActionResult> CreateExamAsync([FromBody] Exam exam)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (exam.Questions == null || exam.Questions.Count == 0)
                return BadRequest("Exam must contain at least one question.");

            if (string.IsNullOrEmpty(exam.CreatedById))
                exam.CreatedById = "00";

            if (exam.ExamId == 0)
                exam.ExamId = int.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(5, 8));

            foreach (var eq in exam.Questions)
            {
                var q = await firebaseClient
                    .Child("Questions")
                    .Child(eq.QuestionId.ToString())
                    .OnceSingleAsync<Question>();
                if (q == null)
                    return BadRequest($"Question {eq.QuestionId} does not exist.");
            }

            if (exam.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0.");
            if (exam.StartTime >= exam.EndTime)
                return BadRequest("StartTime must be before EndTime.");

            if (exam.StartTime.Kind == DateTimeKind.Unspecified)
                exam.StartTime = DateTime.SpecifyKind(exam.StartTime, DateTimeKind.Local);
            if (exam.EndTime.Kind == DateTimeKind.Unspecified)
                exam.EndTime = DateTime.SpecifyKind(exam.EndTime, DateTimeKind.Local);

            exam.StartTime = exam.StartTime.ToUniversalTime();
            exam.EndTime = exam.EndTime.ToUniversalTime();

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

        [HttpPut("{examId}")]
        public async Task<IActionResult> UpdateExamAsync(string examId, [FromBody] Exam exam)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrEmpty(exam.CreatedById))
                exam.CreatedById = "00";

            if (exam.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0.");
            if (exam.StartTime >= exam.EndTime)
                return BadRequest("StartTime must be before EndTime.");

            if (exam.StartTime.Kind == DateTimeKind.Unspecified)
                exam.StartTime = DateTime.SpecifyKind(exam.StartTime, DateTimeKind.Local);
            if (exam.EndTime.Kind == DateTimeKind.Unspecified)
                exam.EndTime = DateTime.SpecifyKind(exam.EndTime, DateTimeKind.Local);

            exam.StartTime = exam.StartTime.ToUniversalTime();
            exam.EndTime = exam.EndTime.ToUniversalTime();

            await firebaseClient
                .Child("Exams")
                .Child(examId)
                .PutAsync(exam);

            return Ok(exam);
        }

        [HttpDelete("{examId}")]
        public async Task<IActionResult> DeleteExamAsync(string examId)
        {
            await firebaseClient
                .Child("Exams")
                .Child(examId)
                .DeleteAsync();
            return Ok();
        }

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

        [HttpGet("class/{idClass}")]
        public async Task<ActionResult<List<Exam>>> GetExamsByClass(int idClass)
        {
            var exams = await GetAllExamsInternal();
            return Ok(exams.Where(e => e.IdClass == idClass).ToList());
        }

        [HttpGet("date")]
        public async Task<ActionResult<List<Exam>>> GetExamsByDate([FromQuery] DateTime date)
        {
            var exams = await GetAllExamsInternal();
            return Ok(exams.Where(e => e.ExamDate.Date == date.Date).ToList());
        }

        [HttpGet("{examId}/questions")]
        public async Task<ActionResult<List<Question>>> GetQuestionsForExam(int examId)
        {
            var exam = await firebaseClient
                .Child("Exams")
                .Child(examId.ToString())
                .OnceSingleAsync<Exam>();

            if (exam == null)
                return NotFound("Exam not found");

            if (exam.Questions == null || exam.Questions.Count == 0)
                return Ok(new List<Question>());

            var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Question/all");

            List<Question> allQuestions;
            try
            {
                allQuestions = JsonConvert.DeserializeObject<List<Question>>(json);
            }
            catch
            {
                return BadRequest("Failed to parse Question list.");
            }

            var questionIdsInExam = exam.Questions.Select(q => q.QuestionId).ToHashSet();
            return Ok(allQuestions.Where(q => questionIdsInExam.Contains(q.QuestionId)).ToList());
        }

        private async Task<List<Result>> GetAllResultsAsync()
        {
            var resultUrl = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Results.json";
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(resultUrl);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<Result>();

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Result>>(json);
                return dict?.Values?.ToList() ?? new List<Result>();
            }
            catch
            {
                return JsonConvert.DeserializeObject<List<Result>>(json)?.Where(r => r != null).ToList() ?? new List<Result>();
            }
        }

        private async Task<List<Exam>> GetAllExamsInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Exams.json";
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(url);

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Exam>>(json);
                return dict?.Values.ToList() ?? new List<Exam>();
            }
            catch
            {
                return JsonConvert.DeserializeObject<List<Exam>>(json)?.Where(e => e != null).ToList() ?? new List<Exam>();
            }
        }

        public class SubmitExamRequest
        {
            public int StudentId { get; set; }
            public int ExamId { get; set; }
            public List<string> Answers { get; set; }
            public int DurationSeconds { get; set; }
        }
        private async Task<List<Result>> GetResultsByStudentIdInternalAsync(int studentId)
        {
            var allResults = await GetAllResultsAsync();
            return allResults.Where(r => r.StudentId == studentId).ToList();
        }

        [HttpGet("{examId}/student/{studentId}/check")]
        public async Task<IActionResult> CheckIfStudentHasTakenExamAsync(int examId, int studentId)
        {
            try
            {
                var results = await GetResultsByStudentIdInternalAsync(studentId);

                if (results != null && results.Any(r => r.ExamId == examId))
                {
                    return Ok(new
                    {
                        hasTaken = true,
                        message = "Bạn đã làm bài này."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        hasTaken = false,
                        message = "Bạn chưa làm bài này. Bạn có thể bắt đầu làm bài."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi kiểm tra kết quả: {ex.Message}");
            }
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

            var vnTz = GetVietnamTimeZone();
            var utcNow = DateTime.UtcNow;

            exam.StartTime = DateTime.SpecifyKind(exam.StartTime, DateTimeKind.Utc);
            exam.EndTime = DateTime.SpecifyKind(exam.EndTime, DateTimeKind.Utc);

            var now = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTz), DateTimeKind.Local);
            var startTimeVN = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(exam.StartTime, vnTz), DateTimeKind.Local);
            var endTimeVN = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(exam.EndTime, vnTz), DateTimeKind.Local);

            if (now > endTimeVN)
                return BadRequest("The exam time is over. You cannot submit anymore.");
            if (now < startTimeVN)
                return BadRequest("The exam has not started yet.");

            using (var httpClient = new HttpClient())
            {
                var studentApiUrl = $"https://api-ielts-cgn8.onrender.com/api/Class/{exam.IdClass}/students";
                var json = await httpClient.GetStringAsync(studentApiUrl);

                var studentsInClass = JsonConvert.DeserializeObject<List<Student>>(json);

                if (!studentsInClass.Any(s => s.StudentId == request.StudentId))
                {
                    return BadRequest("Student is not enrolled in the class for this exam.");
                }
            }

            var existingResults = await GetAllResultsAsync();

            bool alreadySubmitted = existingResults
                .Any(r => r.ExamId == examId && r.StudentId == request.StudentId);

            if (alreadySubmitted)
                return Conflict("Student has already submitted this exam.");

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
                TotalScore = totalScore,
                Remark = $"You got {score} out of {totalScore}",
                Timestamp = DateTime.UtcNow,
                Answers = request.Answers,
                DurationSeconds = request.DurationSeconds
            };

            await firebaseClient
                .Child("Results")
                .Child(result.ResultId.ToString())
                .PutAsync(result);

            return Ok(result);
        }
    }
}

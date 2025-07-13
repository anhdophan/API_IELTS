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

            if (string.IsNullOrEmpty(exam.CreatedById))
                exam.CreatedById = "00"; // Admin mặc định

            // Sinh ExamId tự động nếu chưa có
            if (exam.ExamId == 0)
                exam.ExamId = int.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(5, 8));

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

            // Kiểm tra các trường thời gian (nếu muốn)
            if (exam.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0.");
            if (exam.StartTime >= exam.EndTime)
                return BadRequest("StartTime must be before EndTime.");
            exam.StartTime = DateTime.SpecifyKind(exam.StartTime, DateTimeKind.Local);
            exam.EndTime = DateTime.SpecifyKind(exam.EndTime, DateTimeKind.Local);

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

            if (string.IsNullOrEmpty(exam.CreatedById))
                exam.CreatedById = "00"; // Admin mặc định

            // Kiểm tra các trường thời gian (nếu muốn)
            if (exam.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0.");
            if (exam.StartTime >= exam.EndTime)
                return BadRequest("StartTime must be before EndTime.");

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
        [HttpGet("class/{idClass}")]
        public async Task<ActionResult<List<Exam>>> GetExamsByClass(int idClass)
        {
            var exams = await GetAllExamsInternal();
            var filtered = exams.Where(e => e.IdClass == idClass).ToList();
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
        [HttpGet("{examId}/questions")]
        public async Task<ActionResult<List<Question>>> GetQuestionsForExam(int examId)
        {
            // Lấy exam từ Firebase
            var exam = await firebaseClient
                .Child("Exams")
                .Child(examId.ToString())
                .OnceSingleAsync<Exam>();

            if (exam == null)
                return NotFound("Exam not found");

            if (exam.Questions == null || exam.Questions.Count == 0)
                return Ok(new List<Question>()); // không có câu hỏi nào

            // Gọi API Question/all để lấy toàn bộ câu hỏi
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

            // Lọc theo danh sách questionId trong Exam
            var questionIdsInExam = exam.Questions.Select(q => q.QuestionId).ToHashSet();
            var filteredQuestions = allQuestions
                .Where(q => questionIdsInExam.Contains(q.QuestionId))
                .ToList();

            return Ok(filteredQuestions);
        }


        // Submit Exam Request DTO
        public class SubmitExamRequest
        {
            public int StudentId { get; set; }
            public int ExamId { get; set; }
            public List<string> Answers { get; set; } // Index or input text answers
             public int DurationSeconds { get; set; }
        }

        [HttpPost("{examId}/submit")]
        public async Task<IActionResult> SubmitExamAsync(int examId, [FromBody] SubmitExamRequest request)
        {
            if (examId != request.ExamId)
                return BadRequest("ExamId in route and body must match.");

            // 🔍 Lấy thông tin Exam
            var exam = await firebaseClient
                .Child("Exams")
                .Child(examId.ToString())
                .OnceSingleAsync<Exam>();

            if (exam == null)
                return NotFound("Exam not found");

            if (exam.Questions == null || exam.Questions.Count == 0)
                return BadRequest("Exam has no questions.");

            // ✅ Sử dụng thời gian Local để so sánh với thời gian Local lưu trong Firebase
            var now = DateTime.Now;

            // 🔒 Không cho nộp sau khi bài thi kết thúc
            if (now > exam.EndTime)
                return BadRequest("The exam time is over. You cannot submit anymore.");

            // 🔒 Không cho nộp trước khi bắt đầu
            if (now < exam.StartTime)
                return BadRequest("The exam has not started yet.");

            // 🔒 Kiểm tra StudentId có thuộc Class của Exam không
            var classData = await firebaseClient
                .Child("Classes")
                .Child(exam.IdClass.ToString())
                .OnceSingleAsync<Class>();

            if (classData == null)
                return NotFound("Class for exam not found.");

            if (classData.StudentIds == null || !classData.StudentIds.Contains(request.StudentId))
                return BadRequest("Student is not enrolled in the class for this exam.");

            // 🔁 Không cho nộp lại nếu đã có Result
            var existingResults = await firebaseClient
                .Child("Results")
                .OnceAsync<Result>();

            bool alreadySubmitted = existingResults
                .Any(r => r.Object.ExamId == examId && r.Object.StudentId == request.StudentId);

            if (alreadySubmitted)
                return Conflict("Student has already submitted this exam.");

            // 👉 Chấm điểm
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

            // 🎯 Tạo kết quả
            var result = new Result
            {
                ResultId = int.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(5, 8)),
                StudentId = request.StudentId,
                ExamId = examId,
                Score = score,
                TotalScore = totalScore,
                Remark = $"You got {score} out of {totalScore}",
                Timestamp = now,
                Answers = request.Answers,
                DurationSeconds = request.DurationSeconds
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

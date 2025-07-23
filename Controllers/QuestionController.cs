using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using api.Services;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create
        [HttpPost]
public async Task<IActionResult> CreateQuestionAsync([FromBody] Question question)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

    // Tự động tạo QuestionId mới nếu chưa có
    if (question.QuestionId == 0)
    {
        question.QuestionId = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue);
    }

    if (question.IsMultipleChoice)
    {
        // Kiểm tra đáp án trắc nghiệm
        if (question.Choices == null || question.Choices.Count < 2)
            return BadRequest("Multiple choice question must have at least 2 choices.");

        if (question.CorrectAnswerIndex == null)
            return BadRequest("CorrectAnswerIndex is required.");

        // Nếu người dùng nhập chữ cái (A, B, C...), thì chuyển về số
        var index = question.CorrectAnswerIndex.ToString();
        if (index.Length == 1 && char.IsLetter(index[0]))
        {
            var charIndex = char.ToUpper(index[0]) - 'A';
            if (charIndex < 0 || charIndex >= question.Choices.Count)
                return BadRequest("CorrectAnswerIndex character is out of bounds.");
            question.CorrectAnswerIndex = charIndex;
        }

        if (question.CorrectAnswerIndex < 0 || question.CorrectAnswerIndex >= question.Choices.Count)
            return BadRequest("CorrectAnswerIndex is invalid.");
    }
    else
    {
        if (string.IsNullOrWhiteSpace(question.CorrectInputAnswer))
            return BadRequest("Input question must have a correct answer.");

        question.Choices = new List<string>(); // trống
        question.CorrectAnswerIndex = null;    // không có
    }

    // Gán CreatedById nếu chưa có
    if (string.IsNullOrEmpty(question.CreatedById))
        question.CreatedById = "00"; // Admin mặc định

    await firebaseClient
        .Child("Questions")
        .Child(question.QuestionId.ToString())
        .PutAsync(question);

    return Ok(question);
}

        // Read all
        [HttpGet("all")]
        public async Task<ActionResult<List<Question>>> GetAllQuestionsAsync()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Questions.json";
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);

                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Question>>(json);
                    if (dict != null)
                        return dict.Values.ToList();
                }
                catch { }

                try
                {
                    var list = JsonConvert.DeserializeObject<List<Question>>(json);
                    if (list != null)
                        return list;
                }
                catch { }

                return new List<Question>();
            }
        }

        // Read by id
        [HttpGet("{questionId}")]
        public async Task<ActionResult<Question>> GetQuestionAsync(int questionId)
        {
            var question = await firebaseClient
                .Child("Questions")
                .Child(questionId.ToString())
                .OnceSingleAsync<Question>();

            if (question == null) return NotFound();
            return Ok(question);
        }

        // Update
        [HttpPut("{questionId}")]
        public async Task<IActionResult> UpdateQuestionAsync(int questionId, [FromBody] Question question)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Kiểm tra dữ liệu hợp lệ
            if (question.IsMultipleChoice)
            {
                if (question.Choices == null || question.Choices.Count < 2)
                    return BadRequest("Multiple choice question must have at least 2 choices.");
                if (question.CorrectAnswerIndex == null || question.CorrectAnswerIndex < 0 || question.CorrectAnswerIndex >= question.Choices.Count)
                    return BadRequest("CorrectAnswerIndex is invalid.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(question.CorrectInputAnswer))
                    return BadRequest("Input question must have a correct answer.");
            }

            // Gán CreatedById nếu chưa có
            if (string.IsNullOrEmpty(question.CreatedById))
                question.CreatedById = "00"; // Admin mặc định

            await firebaseClient
                .Child("Questions")
                .Child(questionId.ToString())
                .PutAsync(question);

            return Ok(question);
        }

        // Delete
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestionAsync(int questionId)
        {
            await firebaseClient
                .Child("Questions")
                .Child(questionId.ToString())
                .DeleteAsync();
            return Ok();
        }
[HttpGet("teacher/{teacherId}")]
public async Task<ActionResult<List<Question>>> GetQuestionsByTeacherAsync(string teacherId)
{
    var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Questions.json";

    using var httpClient = new HttpClient();
    var json = await httpClient.GetStringAsync(url);

    List<Question> questions;
    try
    {
        var dict = JsonConvert.DeserializeObject<Dictionary<string, Question>>(json);
        questions = dict?.Values.ToList() ?? new List<Question>();
    }
    catch
    {
        questions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new List<Question>();
    }

    var filtered = questions
        .Where(q => q.CreatedById == teacherId)
        .ToList();

    return Ok(filtered);
}

        // Read by level
        [HttpGet("level/{level}")]
        public async Task<ActionResult<List<Question>>> GetQuestionsByLevel(double level)
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Questions.json";
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);

                var questions = new List<Question>();
                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Question>>(json);
                    if (dict != null)
                        questions = dict.Values.ToList();
                }
                catch
                {
                    questions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new List<Question>();
                }

                var filtered = questions.Where(q => Math.Abs(q.Level - level) < 0.001).ToList();
                return Ok(filtered);
            }
        }
    }
}
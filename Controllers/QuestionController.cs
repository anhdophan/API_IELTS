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
    }
}
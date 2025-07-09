using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using api.Models;
using api.Services;
using Firebase.Database.Query;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        // Create a new result
        [HttpPost]
        public async Task<IActionResult> CreateResultAsync([FromBody] Result result)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Sinh ResultId tự động nếu chưa có
                if (result.ResultId == 0)
                    result.ResultId = int.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(5, 8));

                var existing = await firebaseClient
                    .Child("Results")
                    .Child(result.ResultId.ToString())
                    .OnceSingleAsync<Result>();

                if (existing != null)
                    return Conflict("Result with this ID already exists.");

                await firebaseClient
                    .Child("Results")
                    .Child(result.ResultId.ToString())
                    .PutAsync(result);

                return CreatedAtAction(nameof(GetResultAsync), new { resultId = result.ResultId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating result: {ex.Message}");
            }
        }

        // Update an existing result
        [HttpPut("{resultId}")]
        public async Task<IActionResult> UpdateResultAsync(string resultId, [FromBody] Result result)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (resultId != result.ResultId.ToString())
                return BadRequest("Result ID mismatch.");

            try
            {
                var existing = await firebaseClient
                    .Child("Results")
                    .Child(resultId)
                    .OnceSingleAsync<Result>();

                if (existing == null)
                    return NotFound("Result not found.");

                await firebaseClient
                    .Child("Results")
                    .Child(resultId)
                    .PutAsync(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating result: {ex.Message}");
            }
        }

        // Delete a result
        [HttpDelete("{resultId}")]
        public async Task<IActionResult> DeleteResultAsync(string resultId)
        {
            try
            {
                var existing = await firebaseClient
                    .Child("Results")
                    .Child(resultId)
                    .OnceSingleAsync<Result>();

                if (existing == null)
                    return NotFound("Result not found.");

                await firebaseClient
                    .Child("Results")
                    .Child(resultId)
                    .DeleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting result: {ex.Message}");
            }
        }

        // Get a result by ID
        [HttpGet("{resultId}")]
        public async Task<ActionResult<Result>> GetResultAsync(string resultId)
        {
            try
            {
                var result = await firebaseClient
                    .Child("Results")
                    .Child(resultId)
                    .OnceSingleAsync<Result>();

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching result: {ex.Message}");
            }
        }

        // Get all results
        [HttpGet]
        public async Task<ActionResult<List<Result>>> GetAllResultsAsync()
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var resultList = results.Select(r => r.Object).ToList();

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching all results: {ex.Message}");
            }
        }

        // List results by ExamId
        [HttpGet("exam/{examId}")]
        public async Task<ActionResult<List<Result>>> GetResultsByExamIdAsync(int examId)
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var filteredResults = results
                    .Where(r => r.Object.ExamId == examId)
                    .Select(r => r.Object)
                    .ToList();

                return Ok(filteredResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching results by examId: {ex.Message}");
            }
        }

        // List results by StudentId
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<Result>>> GetResultsByStudentIdAsync(int studentId)
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var filteredResults = results
                    .Where(r => r.Object.StudentId == studentId)
                    .Select(r => r.Object)
                    .ToList();

                return Ok(filteredResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching results by studentId: {ex.Message}");
            }
        }

        // Average score by StudentId
        [HttpGet("student/{studentId}/average")]
        public async Task<ActionResult<double>> GetAverageScoreByStudentAsync(int studentId)
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var studentScores = results
                    .Where(r => r.Object.StudentId == studentId)
                    .Select(r => r.Object.Score)
                    .ToList();

                if (!studentScores.Any())
                    return NotFound("No results found for the student.");

                double average = studentScores.Average();
                return Ok(average);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calculating average: {ex.Message}");
            }
        }

        // Highest score for an Exam
        [HttpGet("exam/{examId}/highest")]
        public async Task<ActionResult<double>> GetHighestScoreByExamAsync(int examId)
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var scores = results
                    .Where(r => r.Object.ExamId == examId)
                    .Select(r => r.Object.Score)
                    .ToList();

                if (!scores.Any())
                    return NotFound("No results found for the exam.");

                return Ok(scores.Max());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calculating highest score: {ex.Message}");
            }
        }

        // Get result by student + exam
        [HttpGet("student/{studentId}/exam/{examId}")]
        public async Task<ActionResult<Result>> GetResultByStudentAndExamAsync(int studentId, int examId)
        {
            try
            {
                var results = await firebaseClient
                    .Child("Results")
                    .OnceAsync<Result>();

                var result = results
                    .Select(r => r.Object)
                    .FirstOrDefault(r => r.StudentId == studentId && r.ExamId == examId);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching result: {ex.Message}");
            }
        }
    }
}

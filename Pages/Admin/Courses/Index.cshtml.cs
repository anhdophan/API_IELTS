using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using api.Models;
using Microsoft.Extensions.Logging;

namespace api.Pages.Admin.Courses
{
    public class IndexModel : AdminPageBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? MinCost { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? MaxCost { get; set; }
        public Dictionary<int, int> CourseClassCountMap { get; set; } = new();

        public List<Course> Courses { get; set; } = new List<Course>();

        [TempData]
        public string StatusMessage { get; set; }

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                HttpResponseMessage response;

                if (FilterDate.HasValue)
                {
                    response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/course/date?date={FilterDate.Value:yyyy-MM-dd}");
                }
                else if (MinCost.HasValue || MaxCost.HasValue)
                {
                    var min = MinCost.HasValue ? MinCost.Value.ToString() : "";
                    var max = MaxCost.HasValue ? MaxCost.Value.ToString() : "";
                    response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/course/cost?minCost={min}&maxCost={max}");
                }
                else
                {
                    response = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/course/all");
                }

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                Courses = JsonConvert.DeserializeObject<List<Course>>(json);

                // Load class counts for each course
                foreach (var course in Courses)
                {
                    var classRes = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/filter?courseId={course.CourseId}");
                    if (classRes.IsSuccessStatusCode)
                    {
                        var classJson = await classRes.Content.ReadAsStringAsync();
                        var classList = JsonConvert.DeserializeObject<List<Class>>(classJson) ?? new();
                        CourseClassCountMap[course.CourseId] = classList.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading courses.");
                StatusMessage = "Error loading courses.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient();

                // 1. Lấy danh sách Class liên kết với CourseId
                var classResponse = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/filter?courseId={id}");
                if (classResponse.IsSuccessStatusCode)
                {
                    var classJson = await classResponse.Content.ReadAsStringAsync();
                    var relatedClasses = JsonConvert.DeserializeObject<List<Class>>(classJson) ?? new List<Class>();

                    foreach (var cls in relatedClasses)
                    {
                        var deleteClassResponse = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{cls.ClassId}");
                        if (!deleteClassResponse.IsSuccessStatusCode)
                        {
                            _logger.LogWarning($"Failed to delete class {cls.ClassId} linked to Course {id}. Status: {deleteClassResponse.StatusCode}");
                        }
                    }
                }

                // 2. Tiến hành xóa Course
                var courseResponse = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/course/{id}");

                if (courseResponse.IsSuccessStatusCode)
                {
                    StatusMessage = $"Course {id} and its related classes deleted successfully.";
                }
                else
                {
                    StatusMessage = $"Failed to delete course {id}. Server returned {courseResponse.StatusCode}.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting course {id}.");
                StatusMessage = $"Unexpected error occurred while deleting course {id}.";
            }

            return RedirectToPage();
        }

    }
}

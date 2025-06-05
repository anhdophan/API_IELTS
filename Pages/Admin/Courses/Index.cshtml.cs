// Updated Index.cshtml.cs for handling deleted Course-Class relations and optimizing logic
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

        public List<Course> Courses { get; set; } = new();
        public Dictionary<int, int> CourseClassCountMap { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

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
                    response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/course/date?date={FilterDate:yyyy-MM-dd}");
                }
                else if (MinCost.HasValue || MaxCost.HasValue)
                {
                    var min = MinCost?.ToString() ?? string.Empty;
                    var max = MaxCost?.ToString() ?? string.Empty;
                    response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/course/cost?minCost={min}&maxCost={max}");
                }
                else
                {
                    response = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/course/all");
                }

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                Courses = JsonConvert.DeserializeObject<List<Course>>(json) ?? new List<Course>();

                // Load class counts
                foreach (var course in Courses)
                {
                    try
                    {
                        var classRes = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/filter?courseId={course.CourseId}");
                        if (classRes.IsSuccessStatusCode)
                        {
                            var classJson = await classRes.Content.ReadAsStringAsync();
                            var classList = JsonConvert.DeserializeObject<List<Class>>(classJson) ?? new();
                            CourseClassCountMap[course.CourseId] = classList.Count;
                        }
                        else
                        {
                            CourseClassCountMap[course.CourseId] = 0;
                        }
                    }
                    catch (Exception classEx)
                    {
                        _logger.LogWarning(classEx, $"Error loading classes for Course {course.CourseId}");
                        CourseClassCountMap[course.CourseId] = 0;
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
                var client = _clientFactory.CreateClient();

                // Attempt to fetch and delete all related classes
                try
                {
                    var classResponse = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/filter?courseId={id}");
                    if (classResponse.IsSuccessStatusCode)
                    {
                        var classJson = await classResponse.Content.ReadAsStringAsync();
                        var relatedClasses = JsonConvert.DeserializeObject<List<Class>>(classJson) ?? new();

                        foreach (var cls in relatedClasses)
                        {
                            var deleteClassResponse = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{cls.ClassId}");
                            if (!deleteClassResponse.IsSuccessStatusCode)
                            {
                                _logger.LogWarning($"Failed to delete Class {cls.ClassId} linked to Course {id}.");
                            }
                        }
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, $"Error retrieving/deleting classes for Course {id}.");
                }

                // Delete course
                var courseRes = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/course/{id}");
                if (courseRes.IsSuccessStatusCode)
                {
                    StatusMessage = $"Course {id} and related classes deleted successfully.";
                }
                else
                {
                    StatusMessage = $"Failed to delete Course {id}. Status: {courseRes.StatusCode}.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting Course {id}.");
                StatusMessage = $"Error deleting Course {id}.";
            }

            return RedirectToPage();
        }
    }
}

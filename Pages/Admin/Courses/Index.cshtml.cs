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

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public List<Course> Courses { get; set; } = new List<Course>();

        [TempData]
        public string StatusMessage { get; set; }

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
                var response = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/course/{id}");

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = $"Course {id} deleted successfully.";
                }
                else
                {
                    StatusMessage = $"Failed to delete course {id}.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting course {id}.");
                StatusMessage = $"Error deleting course {id}.";
            }
            return RedirectToPage();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using api.Models;

namespace api.Pages.Admin.Students
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public List<Student> Students { get; set; } = new List<Student>();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("http://localhost:5035/api/Student/all");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Students = JsonConvert.DeserializeObject<List<Student>>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student list.");
                StatusMessage = "Error loading student list.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.DeleteAsync($"http://localhost:5035/api/Student/{id}");

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = $"Student {id} deleted successfully.";
                }
                else
                {
                    StatusMessage = $"Failed to delete student {id}.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting student {id}.");
                StatusMessage = $"Error deleting student {id}.";
            }

            return RedirectToPage();
        }
    }
}

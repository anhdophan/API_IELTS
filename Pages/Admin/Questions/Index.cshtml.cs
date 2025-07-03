using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Pages.Admin.Questions
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

        public List<Question> Questions { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/Question/all");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Questions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to load questions.");
                StatusMessage = "Error loading questions.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var res = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/Question/{id}");
                if (res.IsSuccessStatusCode)
                    StatusMessage = $"Question {id} deleted successfully.";
                else
                    StatusMessage = $"Failed to delete Question {id}.";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error deleting Question {id}.");
                StatusMessage = $"Error deleting Question {id}.";
            }
            return RedirectToPage();
        }
    }
}
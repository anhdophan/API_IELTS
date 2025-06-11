using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.Admin.Teachers
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

        public List<Teacher> Teachers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchEmail { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                string url = "https://api-ielts-cgn8.onrender.com/api/Teacher";

                if (!string.IsNullOrEmpty(SearchName) || !string.IsNullOrEmpty(SearchEmail))
                {
                    var queryParams = new List<string>();
                    if (!string.IsNullOrEmpty(SearchName)) queryParams.Add($"name={Uri.EscapeDataString(SearchName)}");
                    if (!string.IsNullOrEmpty(SearchEmail)) queryParams.Add($"email={Uri.EscapeDataString(SearchEmail)}");

                    url += "/search?" + string.Join("&", queryParams);
                }
                else
                {
                    url += "/all";
                }

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Teachers = JsonConvert.DeserializeObject<List<Teacher>>(json) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load teachers.");
                StatusMessage = "Error loading teachers.";
            }
        }
    }
}

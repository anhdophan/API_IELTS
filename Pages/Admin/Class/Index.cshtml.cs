using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace api.Pages.Admin.Classes
{
    public class IndexModel : AdminPageBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public List<Class> Classes { get; set; } = new();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var classes = JsonConvert.DeserializeObject<List<Class>>(json);
                    Classes = classes?.Where(c => c != null).ToList() ?? new List<Class>();
                    
                    if (!Classes.Any())
                    {
                        ErrorMessage = "No classes found.";
                    }
                }
                else
                {
                    _logger.LogWarning($"API returned status code: {response.StatusCode}");
                    ErrorMessage = "Unable to load classes. Please try again later.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to API");
                ErrorMessage = "Connection error. Please check your internet connection.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading classes");
                ErrorMessage = "An unexpected error occurred while loading classes.";
            }
        }
    }
}

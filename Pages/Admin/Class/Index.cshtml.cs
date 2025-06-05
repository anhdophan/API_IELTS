using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace api.Pages.Admin.Classes
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

        public List<Class> Classes { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var res = await client.GetAsync("http://localhost:5035/api/Class/all");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadAsStringAsync();
                Classes = JsonConvert.DeserializeObject<List<Class>>(json);
            }
            catch
            {
                Classes = new List<Class>();
                _logger.LogError("Failed to load classes.");
            }
        }
    }
}

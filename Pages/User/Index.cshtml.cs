using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.User
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/Course/all");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Courses = JsonConvert.DeserializeObject<List<Course>>(json) ?? new();
            }
        }
    }
}

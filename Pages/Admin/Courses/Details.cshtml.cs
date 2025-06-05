using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using api.Models;

namespace api.Pages.Admin.Courses
{
    public class DetailsModel : AdminPageBase
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("apiClient");
        }
        
        public Course Course { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var res = await _httpClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/course/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            Course = await res.Content.ReadFromJsonAsync<Course>();
            return Page();
        }
    }
}

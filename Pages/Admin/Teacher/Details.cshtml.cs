using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace api.Pages.Admin.Teachers
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public Teacher Teacher { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{id}");

            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            Teacher = JsonConvert.DeserializeObject<Teacher>(json);

            return Page();
        }
    }
}

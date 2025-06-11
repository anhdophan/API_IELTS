using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace api.Pages.Admin.Teachers
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeleteModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to delete teacher.");
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace api.Pages.Admin.Classes
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeleteModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Class Class { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5035/api/Class/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            Class = JsonConvert.DeserializeObject<Class>(json);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.DeleteAsync($"http://localhost:5035/api/Class/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Delete failed.");
            return Page();
        }
    }
}

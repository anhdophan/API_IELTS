using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Pages.Admin.Registrations
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeleteModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Registration Registration { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetAsync($"http://localhost:5035/api/Registration/{id}");

            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            Registration = JsonConvert.DeserializeObject<Registration>(json);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var res = await client.DeleteAsync($"http://localhost:5035/api/Registration/{id}");

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa đăng ký.");
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Pages.Admin.Registrations
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Registration);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PutAsync($"http://localhost:5035/api/Registration/{Registration.RegistrationId}", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật đăng ký.");
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}

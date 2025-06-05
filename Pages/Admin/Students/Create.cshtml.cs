using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Pages.Admin.Students
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Student Student { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Student);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await client.PostAsync("http://localhost:5035/api/Student", content);

            if (res.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Error creating student.");
            return Page();
        }
    }
}

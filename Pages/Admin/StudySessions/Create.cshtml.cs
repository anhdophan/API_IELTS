using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace api.Pages.Admin.StudySessions
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public StudySession Session { get; set; }

        public SelectList ClassList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadClassesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadClassesAsync();

            if (!ModelState.IsValid)
                return Page();

            Session.DateCreated = DateTime.UtcNow;

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Session);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api-ielts-cgn8.onrender.com/api/StudySession", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to create study session.");
            return Page();
        }

        private async Task LoadClassesAsync()
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
            var classes = JsonConvert.DeserializeObject<List<Class>>(res) ?? new();
            ClassList = new SelectList(classes, "ClassId", "Name");
        }
    }
}

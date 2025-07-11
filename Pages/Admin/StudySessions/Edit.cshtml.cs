using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace api.Pages.Admin.StudySessions
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public StudySession Session { get; set; }

        [BindProperty]
        public string FirebaseKey { get; set; }

        public SelectList ClassList { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            await LoadClassesAsync();

            var client = _clientFactory.CreateClient();
            var res = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/StudySession/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            Session = JsonConvert.DeserializeObject<StudySession>(json);
            FirebaseKey = id;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadClassesAsync();

            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Session);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"https://api-ielts-cgn8.onrender.com/api/StudySession/{FirebaseKey}", content);
            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to update session.");
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

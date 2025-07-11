using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;

namespace api.Pages.Admin.StudySessions
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeleteModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public StudySession Session { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FirebaseKey { get; set; }

        public string ClassName { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            FirebaseKey = id;
            var client = _clientFactory.CreateClient();

            var sessionRes = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/StudySession/{id}");
            if (!sessionRes.IsSuccessStatusCode) return NotFound();

            var json = await sessionRes.Content.ReadAsStringAsync();
            Session = JsonConvert.DeserializeObject<StudySession>(json);

            var classRes = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Class/{Session.ClassID}");
            if (classRes.IsSuccessStatusCode)
            {
                var classJson = await classRes.Content.ReadAsStringAsync();
                var cls = JsonConvert.DeserializeObject<Class>(classJson);
                ClassName = cls?.Name ?? $"Class ID: {Session.ClassID}";
            }
            else
            {
                ClassName = $"Class ID: {Session.ClassID}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient();

            var response = await client.DeleteAsync($"https://api-ielts-cgn8.onrender.com/api/StudySession/{FirebaseKey}");
            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to delete the study session.");
            return Page();
        }
    }
}

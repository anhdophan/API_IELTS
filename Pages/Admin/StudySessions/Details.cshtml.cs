using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;

namespace api.Pages.Admin.StudySessions
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public StudySession Session { get; set; }
        public string ClassName { get; set; }
        public string FirebaseKey { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var client = _clientFactory.CreateClient();

            // Get StudySession
            var sessionRes = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/StudySession/{id}");
            if (!sessionRes.IsSuccessStatusCode) return NotFound();

            var sessionJson = await sessionRes.Content.ReadAsStringAsync();
            Session = JsonConvert.DeserializeObject<StudySession>(sessionJson);
            FirebaseKey = id;

            // Get Class name
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
    }
}

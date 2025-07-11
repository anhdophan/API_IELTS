using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using api.Models;

namespace api.Pages.Admin.StudySessions
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<StudySession> Sessions { get; set; } = new();

        public Dictionary<int, string> ClassMap { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            // Load all classes (for name mapping)
            try
            {
                var classRes = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
                if (classRes.IsSuccessStatusCode)
                {
                    var classJson = await classRes.Content.ReadAsStringAsync();
                    var classList = JsonConvert.DeserializeObject<List<Class>>(classJson);
                    ClassMap = classList.ToDictionary(c => c.ClassId, c => c.Name);
                }
            }
            catch { }

            // Load all study sessions
            try
            {
                var sessionRes = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/StudySession/all");
                sessionRes.EnsureSuccessStatusCode();
                var sessionJson = await sessionRes.Content.ReadAsStringAsync();
                var all = JsonConvert.DeserializeObject<List<StudySession>>(sessionJson) ?? new();

                // Filter if applicable
                Sessions = all.Where(s =>
                    (!ClassId.HasValue || s.ClassID == ClassId) &&
                    (!From.HasValue || s.DateCreated.Date >= From.Value.Date) &&
                    (!To.HasValue || s.DateCreated.Date <= To.Value.Date)
                ).OrderByDescending(s => s.DateCreated).ToList();
            }
            catch
            {
                Sessions = new();
            }
        }
    }
}

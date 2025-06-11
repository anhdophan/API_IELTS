using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace api.Pages.Admin.Classes
{
    public class IndexModel : AdminPageBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TeacherId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndTo { get; set; }

        public List<Class> Classes { get; set; } = new();

        public Dictionary<int, string> TeacherNameMap { get; set; } = new();

        public  async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            try
            {
                // 1. Load all teachers
                var teacherRes = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/Teacher/all");
                if (teacherRes.IsSuccessStatusCode)
                {
                    var teacherJson = await teacherRes.Content.ReadAsStringAsync();
                    var teacherList = JsonConvert.DeserializeObject<List<Teacher>>(teacherJson) ?? new();
                    TeacherNameMap = teacherList.ToDictionary(t => t.TeacherId, t => t.Name);
                }

                // 2. Load all classes
                var classRes = await client.GetAsync("https://api-ielts-cgn8.onrender.com/api/Class/all");
                classRes.EnsureSuccessStatusCode();
                var classJson = await classRes.Content.ReadAsStringAsync();
                var allClasses = JsonConvert.DeserializeObject<List<Class>>(classJson) ?? new();

                // 3. Filter
                Classes = allClasses
                    .Where(c =>
                        (string.IsNullOrEmpty(SearchName) || c.Name?.ToLower().Contains(SearchName.ToLower()) == true) &&
                        (!TeacherId.HasValue || c.TeacherId == TeacherId) &&
                        (!StartFrom.HasValue || c.StartDate.Date >= StartFrom.Value.Date) &&
                        (!EndTo.HasValue || c.EndDate.Date <= EndTo.Value.Date)
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data in Class Index");
                Classes = new();
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Pages.Admin.Students
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeleteModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Student Student { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetAsync($"http://localhost:5035/api/Student/{id}");

            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            Student = JsonConvert.DeserializeObject<Student>(json);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient();
            var res = await client.DeleteAsync($"http://localhost:5035/api/Student/{Student.StudentId}");

            if (res.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Error deleting student.");
            return Page();
        }
    }
}

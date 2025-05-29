using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using api.Models;

namespace Admin.Students
{
    public class IndexModel : PageModel
    {
        public List<Student> Students { get; set; }

        public async Task OnGetAsync()
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync("http://localhost:5035/api/Student");
            Students = JsonSerializer.Deserialize<List<Student>>(response);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            using var client = new HttpClient();
            await client.DeleteAsync($"http://localhost:5035/api/Student/{id}");
            return RedirectToPage();
        }
    }
}
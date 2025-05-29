using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using api.Models;


namespace api.Pages.Admin.Courses
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public Course Course { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var http = new HttpClient();
            var res = await http.GetAsync($"/api/course/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();
            Course = await res.Content.ReadFromJsonAsync<Course>();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var http = new HttpClient();
            var res = await http.DeleteAsync($"/api/course/{Course.CourseId}");
            if (!res.IsSuccessStatusCode) return BadRequest();
            return RedirectToPage("Index");
        }
    }
}
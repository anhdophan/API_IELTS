using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace api.Pages.Admin.Teachers
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Teacher Teacher { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var res = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{id}");

            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            Teacher = JsonConvert.DeserializeObject<Teacher>(json);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile AvatarUpload)
        {
            if (!ModelState.IsValid)
                return Page();

            // Xử lý upload avatar
            if (AvatarUpload != null && AvatarUpload.Length > 0)
            {
                // TODO: Upload lên cloud hoặc lưu server, ở đây chỉ demo lấy tên file
                Teacher.Avatar = "/uploads/" + System.IO.Path.GetFileName(AvatarUpload.FileName);
            }

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Teacher);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"https://api-ielts-cgn8.onrender.com/api/Teacher/{Teacher.TeacherId}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to update teacher.");
            return Page();
        }
    }
}

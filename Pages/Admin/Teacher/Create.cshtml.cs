using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace api.Pages.Admin.Teachers
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Teacher Teacher { get; set; }

        public void OnGet() { }

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

            var response = await client.PostAsync("https://api-ielts-cgn8.onrender.com/api/Teacher", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to create teacher.");
            return Page();
        }
    }
}

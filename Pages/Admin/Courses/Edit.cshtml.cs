// Pages/Admin/Courses/Edit.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using System.Collections.Generic;

namespace api.Pages.Admin.Courses
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public EditModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("apiClient");
            _config = config;
        }

       [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public Course Course { get; set; }

        [BindProperty]
        public IFormFile ImageUpload { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var res = await _httpClient.GetAsync($"api/course/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            Course = await res.Content.ReadFromJsonAsync<Course>();
            
            // Đảm bảo Images không null
            if (Course.Images == null)
                Course.Images = new List<string>();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Gán ID được truyền vào cho Course
            Course.CourseId = Id;

            // Xóa validation lỗi do không post lên từ form (nếu có)
            ModelState.Remove("Course.Images");
            ModelState.Remove("ImageUpload");

            if (!ModelState.IsValid)
                return Page();

            string imageUrl = Course.Images?.Count > 0 ? Course.Images[0] : "https://via.placeholder.com/600x400.png?text=No+Image";

            if (ImageUpload != null)
            {
                var account = new Account(
                    _config["Cloudinary:CloudName"],
                    _config["Cloudinary:ApiKey"],
                    _config["Cloudinary:ApiSecret"]
                );
                var cloudinary = new Cloudinary(account);

                await using var stream = ImageUpload.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(ImageUpload.FileName, stream),
                    Folder = "courses"
                };
                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    imageUrl = uploadResult.SecureUrl.ToString();
                }
            }

            Course.Images = new List<string> { imageUrl };

            var res = await _httpClient.PutAsJsonAsync($"api/course/{Course.CourseId}", Course);
            if (!res.IsSuccessStatusCode)
                return BadRequest();

            return RedirectToPage("Index");
        }
    }
}

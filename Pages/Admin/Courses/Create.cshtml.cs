using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Collections.Generic;
using System.IO;

namespace api.Pages.Admin.Courses
{
    public class CreateModel : AdminPageBase
    {
        private readonly IConfiguration _config;

        public CreateModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public Course Course { get; set; }

        [BindProperty]
        public IFormFile ImageUpload { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string imageUrl = "https://via.placeholder.com/600x400.png?text=No+Image";

            if (ImageUpload != null && ImageUpload.Length > 0)
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

            // Gán ảnh vào Course trước khi validate
            Course.Images = new List<string> { imageUrl };

            // Xóa lỗi liên quan đến Images và ImageUpload nếu có trước đó
            ModelState.Remove("Course.Images");
            ModelState.Remove("ImageUpload");

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"{entry.Key}: {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            using var httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(Course);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5035/api/Course", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Không thể tạo khóa học. Hãy kiểm tra lại dữ liệu.");
            return Page();
        }
    }
}

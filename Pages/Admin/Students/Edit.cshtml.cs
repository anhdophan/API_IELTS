using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;

namespace api.Pages.Admin.Students
{
    public class EditModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IConfiguration config, IHttpClientFactory clientFactory, ILogger<EditModel> logger)
        {
            _config = config;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [BindProperty]
        public Student Student { get; set; }

        [BindProperty]
        public IFormFile AvatarUpload { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            Student = JsonConvert.DeserializeObject<Student>(json);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                    return Page();
                }

                // If a new avatar is uploaded, upload it and set Student.Avatar
                if (AvatarUpload != null && AvatarUpload.Length > 0)
                {
                    var account = new Account(
                        _config["Cloudinary:CloudName"],
                        _config["Cloudinary:ApiKey"],
                        _config["Cloudinary:ApiSecret"]
                    );
                    var cloudinary = new Cloudinary(account);

                    await using var stream = AvatarUpload.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(AvatarUpload.FileName, stream),
                        Folder = "students"
                    };
                    var uploadResult = await cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Student.Avatar = uploadResult.SecureUrl.ToString();
                    }
                    else
                    {
                        _logger.LogError("Cloudinary upload failed: {@Result}", uploadResult);
                        ModelState.AddModelError(string.Empty, "Avatar upload failed.");
                        return Page();
                    }
                }
                // If no new file, keep the existing avatar (from the readonly input)
                // If no new file, keep the existing avatar
                else if (string.IsNullOrEmpty(Student.Avatar))
                {
                    // Try get old avatar from API
                    var oldClient = _clientFactory.CreateClient();
                    var oldResponse = await oldClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{Student.StudentId}");
                    if (oldResponse.IsSuccessStatusCode)
                    {
                        var oldJson = await oldResponse.Content.ReadAsStringAsync();
                        var oldStudent = JsonConvert.DeserializeObject<Student>(oldJson);
                        Student.Avatar = oldStudent?.Avatar;
                        Student.Password ??= oldStudent?.Password;
                    }
                    else
                    {
                        ModelState.AddModelError("Student.Avatar", "Avatar is required.");
                        return Page();
                    }
                }


                // Make sure Password is not lost (if you don't want to force re-entry)
                if (string.IsNullOrEmpty(Student.Password))
                {
                    var oldClient = _clientFactory.CreateClient();
                    var oldResponse = await oldClient.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{Student.StudentId}");
                    if (oldResponse.IsSuccessStatusCode)
                    {
                        var oldJson = await oldResponse.Content.ReadAsStringAsync();
                        var oldStudent = JsonConvert.DeserializeObject<Student>(oldJson);
                        Student.Password = oldStudent?.Password;
                    }
                }

                var client = _clientFactory.CreateClient();
                var json = JsonConvert.SerializeObject(Student);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"https://api-ielts-cgn8.onrender.com/api/Student/{Student.StudentId}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToPage("Index");

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update student. Status: {Status}, Response: {Response}", response.StatusCode, errorContent);
                ModelState.AddModelError(string.Empty, "Error updating student.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in OnPostAsync");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return Page();
            }
        }
    }
}

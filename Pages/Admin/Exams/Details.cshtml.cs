using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace api.Pages.Admin.Exams
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public Exam Exam { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"https://api-ielts-cgn8.onrender.com/api/Exam/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var json = await response.Content.ReadAsStringAsync();
            Exam = JsonConvert.DeserializeObject<Exam>(json);

            return Page();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using api.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace api.Pages.Admin.Classes
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Class Class { get; set; }

        [BindProperty]
        public List<string> DayOfWeeks { get; set; }
        [BindProperty]
        public List<string> StartTimes { get; set; }
        [BindProperty]
        public List<string> EndTimes { get; set; }

        public SelectList CourseList { get; set; }
        public SelectList TeacherList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDropdownsAsync();

            if (DayOfWeeks.Count != StartTimes.Count || StartTimes.Count != EndTimes.Count)
            {
                ModelState.AddModelError(string.Empty, "Schedule data is invalid.");
                return Page();
            }

            Class.Schedule = new();
            for (int i = 0; i < DayOfWeeks.Count; i++)
            {
                Class.Schedule.Add(new ClassSchedule
                {
                    DayOfWeek = DayOfWeeks[i],
                    StartTime = StartTimes[i],
                    EndTime = EndTimes[i]
                });
            }

            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(Class);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api-ielts-cgn8.onrender.com//api/Class", content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, "Failed to create class.");
            return Page();
        }

        private async Task LoadDropdownsAsync()
        {
            var client = _clientFactory.CreateClient();
            var courseRes = await client.GetStringAsync("https://api-ielts-cgn8.onrender.com/api/Course/all");
            var courses = JsonConvert.DeserializeObject<List<Course>>(courseRes) ?? new();
            CourseList = new SelectList(courses, "CourseId", "Name");

            var teacherRes = await client.GetStringAsync("https://api-ielts-cgn8.onrender.com//api/Teacher/all");
            var teachers = JsonConvert.DeserializeObject<List<Teacher>>(teacherRes) ?? new();
            teachers.Insert(0, new Teacher { TeacherId = 0, Name = "Chưa có giảng viên" });
            TeacherList = new SelectList(teachers, "TeacherId", "Name");
        }
    }
}

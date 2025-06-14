using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using api.Services;
using api.Models;
using Newtonsoft.Json;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly FirebaseClient firebaseClient = FirebaseService.Client;

        public class RegistrationRequest
        {
            public bool Submit { get; set; }
            public Student Student { get; set; }
            public int CourseId { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterStudentAsync([FromBody] RegistrationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!request.Submit)
                return BadRequest("Registration not submitted.");

            var student = request.Student;
            student.Username = student.Email;
            student.Password = student.Email + "123";
            if (student.Score == 0) student.Score = 0;
            if (string.IsNullOrEmpty(student.Class)) student.Class = "";

            await firebaseClient
                .Child("Students")
                .Child(student.StudentId.ToString())
                .PutAsync(student);

            var registration = new Registration
            {
                RegistrationId = Guid.NewGuid().GetHashCode(),
                StudentId = student.StudentId,
                CourseId = request.CourseId,
                RegistrationDate = DateTime.UtcNow,
                Email = student.Email,
                Status = RegistrationStatus.Unread
            };

            await firebaseClient
                .Child("Registrations")
                .Child(registration.RegistrationId.ToString())
                .PutAsync(registration);

            return Ok(new { student, registration });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegistrationAsync([FromBody] Registration registration)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await firebaseClient
                .Child("Registrations")
                .Child(registration.RegistrationId.ToString())
                .PutAsync(registration);

            return Ok(registration);
        }

        [HttpPut("{registrationId}")]
        public async Task<IActionResult> UpdateRegistrationAsync(string registrationId, [FromBody] Registration registration)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exist = await firebaseClient
                .Child("Registrations")
                .Child(registrationId)
                .OnceSingleAsync<Registration>();

            if (exist == null)
                return NotFound($"Registration {registrationId} not found.");

            await firebaseClient
                .Child("Registrations")
                .Child(registrationId)
                .PutAsync(registration);

            return Ok(registration);
        }

        [HttpDelete("{registrationId}")]
        public async Task<IActionResult> DeleteRegistrationAsync(string registrationId)
        {
            var exist = await firebaseClient
                .Child("Registrations")
                .Child(registrationId)
                .OnceSingleAsync<Registration>();

            if (exist == null)
                return NotFound($"Registration {registrationId} not found.");

            await firebaseClient
                .Child("Registrations")
                .Child(registrationId)
                .DeleteAsync();

            return Ok();
        }

        [HttpGet("{registrationId}")]
        public async Task<ActionResult<Registration>> GetRegistrationAsync(string registrationId)
        {
            var registration = await firebaseClient
                .Child("Registrations")
                .Child(registrationId)
                .OnceSingleAsync<Registration>();

            if (registration == null)
                return NotFound();

            return Ok(registration);
        }

        [HttpGet]
        public async Task<ActionResult<List<Registration>>> GetAllRegistrationsAsync()
        {
            try
            {
                var registrations = await GetAllRegistrationsInternal();
                return Ok(registrations);
            }
            catch
            {
                return BadRequest("Failed to get registrations.");
            }
        }

        // Filter registrations by CourseId
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<List<Registration>>> GetRegistrationsByCourseAsync(int courseId)
        {
            var registrations = await GetAllRegistrationsInternal();
            var filtered = registrations.Where(r => r.CourseId == courseId).ToList();
            return Ok(filtered);
        }

        // Filter registrations by StudentId
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<Registration>>> GetRegistrationsByStudentAsync(int studentId)
        {
            var registrations = await GetAllRegistrationsInternal();
            var filtered = registrations.Where(r => r.StudentId == studentId).ToList();
            return Ok(filtered);
        }

        // Filter registrations by Status
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<Registration>>> GetRegistrationsByStatusAsync(RegistrationStatus status)
        {
            var registrations = await GetAllRegistrationsInternal();
            var filtered = registrations.Where(r => r.Status == status).ToList();
            return Ok(filtered);
        }

        // List all classes of a course for student to choose
        [HttpGet("course/{courseId}/classes")]
        public async Task<ActionResult<List<Class>>> GetClassesByCourseAsync(int courseId)
        {
            var classes = await firebaseClient
                .Child("Classes")
                .OnceAsync<Class>();

            var classList = new List<Class>();
            foreach (var c in classes)
            {
                if (c.Object.CourseId == courseId)
                    classList.Add(c.Object);
            }
            return Ok(classList);
        }

        // Register student for a specific class
        public class RegistrationWithClassRequest
        {
            public bool Submit { get; set; }
            public Student Student { get; set; }
            public int CourseId { get; set; }
            public int ClassId { get; set; }
        }

        [HttpPost("register-with-class")]
        public async Task<IActionResult> RegisterStudentWithClassAsync([FromBody] RegistrationWithClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!request.Submit)
                return BadRequest("Registration not submitted.");

            var student = request.Student;
            student.Username = student.Email;
            student.Password = student.Email + "123";
            if (student.Score == 0) student.Score = 0;
            if (string.IsNullOrEmpty(student.Class)) student.Class = "";

            await firebaseClient
                .Child("Students")
                .Child(student.StudentId.ToString())
                .PutAsync(student);

            var registration = new Registration
            {
                RegistrationId = Guid.NewGuid().GetHashCode(),
                StudentId = student.StudentId,
                CourseId = request.CourseId,
                RegistrationDate = DateTime.UtcNow,
                Email = student.Email,
                Status = RegistrationStatus.Unread
            };

            await firebaseClient
                .Child("Registrations")
                .Child(registration.RegistrationId.ToString())
                .PutAsync(registration);

            var classObj = await firebaseClient
                .Child("Classes")
                .Child(request.ClassId.ToString())
                .OnceSingleAsync<Class>();

            if (classObj != null)
            {
                if (classObj.StudentIds == null)
                    classObj.StudentIds = new List<int>();

                if (!classObj.StudentIds.Contains(student.StudentId))
                    classObj.StudentIds.Add(student.StudentId);

                await firebaseClient
                    .Child("Classes")
                    .Child(request.ClassId.ToString())
                    .PutAsync(classObj);
            }

            return Ok(new { student, registration, classId = request.ClassId });
        }

        // Private helper: get all registrations from Firebase (support Dictionary or List JSON)
        private async Task<List<Registration>> GetAllRegistrationsInternal()
        {
            var url = "https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/Registrations.json";
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var json = await httpClient.GetStringAsync(url);

                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Registration>>(json);
                    if (dict != null)
                        return dict.Values.ToList();
                }
                catch { }

                try
                {
                    var list = JsonConvert.DeserializeObject<List<Registration>>(json);
                    if (list != null)
                        return list;
                }
                catch { }

                throw new Exception("Failed to parse data as Dictionary<string, Registration> or List<Registration>.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace api.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string StudyingCourse { get; set; }
        public double Score { get; set; }
        [JsonProperty("class")]
        public string ClassId { get; set; }

        public string Avatar { get; set; }
    }
}
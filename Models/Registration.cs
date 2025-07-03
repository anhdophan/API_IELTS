using System;

namespace api.Models
{
    public enum RegistrationStatus
    {
        Unread,
        Confirm,
        Deny
    }

    public class Registration
    {
        public int RegistrationId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; } // Thêm dòng này
        public string StudentName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public RegistrationStatus Status { get; set; }
    }
}
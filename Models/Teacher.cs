using System.Collections.Generic;

namespace api.Models
{
          public class Teacher
          {
                    public int TeacherId { get; set; }
                    public string Name { get; set; }
                    public string Email { get; set; }
                    public string PhoneNumber { get; set; }
                    public string Username { get; set; }
                    public string Password { get; set; }        
                    public List<int> ClassIds { get; set; } // List of Class IDs that the teacher is associated with
                    public List<int> CourseIds { get; set; } // List of Course IDs that the teacher is associated with
                    public List<int> ExamIds { get; set; } // List of Exam IDs that the teacher is associated with
    }
}
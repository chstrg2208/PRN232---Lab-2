using System;
using System.Collections.Generic;

namespace PRN232.LMS.Repositories.Entities
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? RollNumber { get; set; }
        public string? PhoneNumber { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}

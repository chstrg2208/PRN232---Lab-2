using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.Business
{
    public class SemesterBusinessModel
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CourseBusinessModel> Courses { get; set; } = new();
    }

    public class SubjectBusinessModel
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credit { get; set; }
    }

    public class CourseBusinessModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public SemesterBusinessModel? Semester { get; set; }
        public List<EnrollmentBusinessModel> Enrollments { get; set; } = new();
    }

    public class StudentBusinessModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? RollNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public List<EnrollmentBusinessModel> Enrollments { get; set; } = new();
    }

    public class EnrollmentBusinessModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public StudentBusinessModel? Student { get; set; }
        public CourseBusinessModel? Course { get; set; }
    }
}

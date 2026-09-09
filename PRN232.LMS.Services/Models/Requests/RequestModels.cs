using System;
using System.ComponentModel.DataAnnotations;
using PRN232.LMS.Services.Validators;

namespace PRN232.LMS.Services.Models.Requests
{
    public class QueryParameters
    {
        public string? Search { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string? Fields { get; set; }
        public string? Expand { get; set; }
    }

    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [FptuRollNumber]
        [RegularExpression(@"^(SE|CE|IA|HE|HS|SS|MC|GD)\d{5,}$", ErrorMessage = "Roll number must match FPTU pattern (e.g. SE19886, CE18793)")]
        public string? RollNumber { get; set; }
    }

    public class UpdateStudentRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [FptuRollNumber]
        [RegularExpression(@"^(SE|CE|IA|HE|HS|SS|MC|GD)\d{5,}$", ErrorMessage = "Roll number must match FPTU pattern (e.g. SE19886, CE18793)")]
        public string? RollNumber { get; set; }
    }

    public class CreateEnrollmentRequest
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }

    public class UpdateEnrollmentRequest
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }

    public class CreateCourseRequest
    {
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public int SemesterId { get; set; }
    }

    public class UpdateCourseRequest
    {
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public int SemesterId { get; set; }
    }

    public class CreateSemesterRequest
    {
        [Required]
        [MaxLength(100)]
        public string SemesterName { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class UpdateSemesterRequest
    {
        [Required]
        [MaxLength(100)]
        public string SemesterName { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class CreateSubjectRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string SubjectName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Credit must be between 1 and 10")]
        public int Credit { get; set; }
    }

    public class UpdateSubjectRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string SubjectName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Credit must be between 1 and 10")]
        public int Credit { get; set; }
    }
}

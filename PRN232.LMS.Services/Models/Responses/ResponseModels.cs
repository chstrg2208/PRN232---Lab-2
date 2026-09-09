using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PRN232.LMS.Services.Models.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Request processed successfully";
        public T? Data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaginationMetadata? Pagination { get; set; }

        public List<string>? Errors { get; set; }

        public ApiResponse() { }

        public static ApiResponse<T> Ok(T? data = default, string message = "Request processed successfully", PaginationMetadata? pagination = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Pagination = pagination,
                Errors = null
            };
        }

        public static ApiResponse<T> Fail(string message, object? errors = null)
        {
            List<string>? errList = null;
            if (errors != null)
            {
                if (errors is IEnumerable<string> list)
                {
                    errList = new List<string>(list);
                }
                else
                {
                    errList = new List<string> { errors.ToString() ?? "An error occurred" };
                }
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Pagination = null,
                Errors = errList
            };
        }
    }

    public class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = new();
    }

    public class SemesterResponse
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CourseSummaryResponse>? Courses { get; set; }
    }

    public class SemesterSummaryResponse
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
    }

    public class SubjectResponse
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credit { get; set; }
    }

    public class CourseResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public SemesterSummaryResponse? Semester { get; set; }
        public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
    }

    public class CourseSummaryResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
    }

    public class StudentResponse
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? RollNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
    }

    public class StudentSummaryResponse
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? RollNumber { get; set; }
    }

    public class EnrollmentResponse
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public StudentSummaryResponse? Student { get; set; }
        public CourseSummaryResponse? Course { get; set; }
    }

    public class EnrollmentSummaryResponse
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

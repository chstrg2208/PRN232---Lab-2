using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses(
            [FromQuery] QueryParameters queryParams,
            [FromHeader(Name = "X-Request-Id")] string? requestId = null)
        {
            var result = await _courseService.GetCoursesAsync(queryParams);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(result.Items, "Courses retrieved successfully", result.Pagination));
        }

        [HttpGet("{id:int}", Name = "GetCourseById")]
        public async Task<IActionResult> GetCourseById(
            [FromRoute] int id,
            [FromQuery] string? fields,
            [FromQuery] string? expand)
        {
            var result = await _courseService.GetCourseByIdAsync(id, fields, expand);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {id} was not found"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Course retrieved successfully"));
        }

        // Nested resource: /api/v1/courses/{courseId}/students
        [HttpGet("{courseId:int}/students")]
        public async Task<IActionResult> GetStudentsByCourse([FromRoute] int courseId)
        {
            var students = await _courseService.GetStudentsByCourseIdAsync(courseId);
            if (students == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {courseId} was not found"));
            }
            return Ok(ApiResponse<IEnumerable<StudentSummaryResponse>>.Ok(students, "Course students retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _courseService.CreateCourseAsync(request);
            return CreatedAtRoute("GetCourseById", new { id = result.CourseId, version = "1.0" }, ApiResponse<CourseResponse>.Ok(result, "Course created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse([FromRoute] int id, [FromBody] UpdateCourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _courseService.UpdateCourseAsync(id, request);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {id} was not found"));
            }
            return Ok(ApiResponse<CourseResponse>.Ok(result, "Course updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse([FromRoute] int id)
        {
            var success = await _courseService.DeleteCourseAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Course with ID {id} was not found"));
            }
            return Ok(ApiResponse<object?>.Ok(null, "Course deleted successfully"));
        }
    }

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/semesters")]
    public class SemestersController : ControllerBase
    {
        private readonly ISemesterService _semesterService;

        public SemestersController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSemesters([FromQuery] QueryParameters queryParams)
        {
            var result = await _semesterService.GetSemestersAsync(queryParams);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(result.Items, "Semesters retrieved successfully", result.Pagination));
        }

        [HttpGet("{id:int}", Name = "GetSemesterById")]
        public async Task<IActionResult> GetSemesterById(
            [FromRoute] int id,
            [FromQuery] string? fields,
            [FromQuery] string? expand)
        {
            var result = await _semesterService.GetSemesterByIdAsync(id, fields, expand);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} was not found"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Semester retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSemester([FromBody] CreateSemesterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _semesterService.CreateSemesterAsync(request);
            return CreatedAtRoute("GetSemesterById", new { id = result.SemesterId, version = "1.0" }, ApiResponse<SemesterResponse>.Ok(result, "Semester created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSemester([FromRoute] int id, [FromBody] UpdateSemesterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _semesterService.UpdateSemesterAsync(id, request);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} was not found"));
            }
            return Ok(ApiResponse<SemesterResponse>.Ok(result, "Semester updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSemester([FromRoute] int id)
        {
            var success = await _semesterService.DeleteSemesterAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Semester with ID {id} was not found"));
            }
            return Ok(ApiResponse<object?>.Ok(null, "Semester deleted successfully"));
        }
    }

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/subjects")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjects([FromQuery] QueryParameters queryParams)
        {
            var result = await _subjectService.GetSubjectsAsync(queryParams);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(result.Items, "Subjects retrieved successfully", result.Pagination));
        }

        [HttpGet("{id:int}", Name = "GetSubjectById")]
        public async Task<IActionResult> GetSubjectById([FromRoute] int id, [FromQuery] string? fields)
        {
            var result = await _subjectService.GetSubjectByIdAsync(id, fields);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Subject with ID {id} was not found"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Subject retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _subjectService.CreateSubjectAsync(request);
            return CreatedAtRoute("GetSubjectById", new { id = result.SubjectId, version = "1.0" }, ApiResponse<SubjectResponse>.Ok(result, "Subject created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubject([FromRoute] int id, [FromBody] UpdateSubjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _subjectService.UpdateSubjectAsync(id, request);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Subject with ID {id} was not found"));
            }
            return Ok(ApiResponse<SubjectResponse>.Ok(result, "Subject updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubject([FromRoute] int id)
        {
            var success = await _subjectService.DeleteSubjectAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Subject with ID {id} was not found"));
            }
            return Ok(ApiResponse<object?>.Ok(null, "Subject deleted successfully"));
        }
    }
}

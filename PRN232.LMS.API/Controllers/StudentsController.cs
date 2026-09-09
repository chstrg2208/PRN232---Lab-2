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
    [Route("api/v{version:apiVersion}/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents(
            [FromQuery] QueryParameters queryParams,
            [FromHeader(Name = "X-Request-Id")] string? requestId = null)
        {
            var result = await _studentService.GetStudentsAsync(queryParams);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(result.Items, "Students retrieved successfully", result.Pagination));
        }

        [HttpGet("{id:int}", Name = "GetStudentById")]
        public async Task<IActionResult> GetStudentById(
            [FromRoute] int id,
            [FromQuery] string? fields,
            [FromQuery] string? expand)
        {
            var result = await _studentService.GetStudentByIdAsync(id, fields, expand);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} was not found"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Student retrieved successfully"));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _studentService.CreateStudentAsync(request);
            return CreatedAtRoute("GetStudentById", new { id = result.StudentId, version = "1.0" }, ApiResponse<StudentResponse>.Ok(result, "Student created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateStudent([FromRoute] int id, [FromBody] UpdateStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _studentService.UpdateStudentAsync(id, request);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} was not found"));
            }
            return Ok(ApiResponse<StudentResponse>.Ok(result, "Student updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent([FromRoute] int id)
        {
            var success = await _studentService.DeleteStudentAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} was not found"));
            }
            return Ok(ApiResponse<object?>.Ok(null, "Student deleted successfully"));
        }
    }
}

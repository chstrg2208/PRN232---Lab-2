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
    [Route("api/v{version:apiVersion}/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEnrollments(
            [FromQuery] QueryParameters queryParams,
            [FromHeader(Name = "X-Request-Id")] string? requestId = null)
        {
            var result = await _enrollmentService.GetEnrollmentsAsync(queryParams);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(result.Items, "Enrollments retrieved successfully", result.Pagination));
        }

        [HttpGet("{id:int}", Name = "GetEnrollmentById")]
        public async Task<IActionResult> GetEnrollmentById(
            [FromRoute] int id,
            [FromQuery] string? fields,
            [FromQuery] string? expand)
        {
            var result = await _enrollmentService.GetEnrollmentByIdAsync(id, fields, expand);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Enrollment with ID {id} was not found"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Enrollment retrieved successfully"));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _enrollmentService.CreateEnrollmentAsync(request);
            return CreatedAtRoute("GetEnrollmentById", new { id = result.EnrollmentId, version = "1.0" }, ApiResponse<EnrollmentResponse>.Ok(result, "Enrollment created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateEnrollment([FromRoute] int id, [FromBody] UpdateEnrollmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid input data", ModelState));
            }
            var result = await _enrollmentService.UpdateEnrollmentAsync(id, request);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Enrollment with ID {id} was not found"));
            }
            return Ok(ApiResponse<EnrollmentResponse>.Ok(result, "Enrollment updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEnrollment([FromRoute] int id)
        {
            var success = await _enrollmentService.DeleteEnrollmentAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail($"Enrollment with ID {id} was not found"));
            }
            return Ok(ApiResponse<object?>.Ok(null, "Enrollment deleted successfully"));
        }
    }
}

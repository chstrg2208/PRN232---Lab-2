using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/students")]
    public class StudentsV2Controller : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsV2Controller(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsV2(
            [FromQuery] QueryParameters queryParams,
            [FromHeader(Name = "X-Request-Id")] string? requestId = null)
        {
            var result = await _studentService.GetStudentsAsync(queryParams);

            // In Version 2, we return enhanced metadata for API v2 consumers
            var v2Items = result.Items.Select(item =>
            {
                if (item is IDictionary<string, object?> dict)
                {
                    dict["apiVersion"] = "2.0";
                    return (object)dict;
                }
                return item;
            });

            return Ok(ApiResponse<object>.Ok(new
            {
                version = "v2.0",
                requestId = requestId ?? Guid.NewGuid().ToString("N"),
                students = v2Items
            }, "Students retrieved successfully via API v2", result.Pagination));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentByIdV2([FromRoute] int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id, null, "enrollments");
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Student with ID {id} was not found"));
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                version = "v2.0",
                student = result
            }, "Student retrieved successfully via API v2"));
        }
    }
}

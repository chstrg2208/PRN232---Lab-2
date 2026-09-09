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
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid registration data", ModelState));
            }

            var result = await _authService.RegisterAsync(request);
            if (result == null)
            {
                return Conflict(ApiResponse<object>.Fail($"Username '{request.Username}' is already taken"));
            }

            return StatusCode(201, ApiResponse<AuthResponse>.Ok(result, "User registered successfully with assigned tokens"));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid credentials provided", ModelState));
            }

            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid username or password"));
            }

            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid refresh token request", ModelState));
            }

            var result = await _authService.RefreshTokenAsync(request);
            if (result == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token"));
            }

            return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully"));
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var success = await _authService.RevokeTokenAsync(refreshToken);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("Token not found or already revoked"));
            }

            return Ok(ApiResponse<object?>.Ok(null, "Token revoked successfully"));
        }
    }
}

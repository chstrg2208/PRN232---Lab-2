using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;

namespace PRN232.LMS.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<RefreshToken> _refreshTokenRepo;
        private readonly IConfiguration _configuration;

        public AuthService(
            IRepository<User> userRepo,
            IRepository<RefreshToken> refreshTokenRepo,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepo.GetQueryable()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (existingUser != null)
            {
                return null;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 10);

            var user = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = passwordHash,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "Student" : request.Role.Trim()
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            // Automatically assign and generate tokens upon registration
            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetQueryable()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                return null;
            }

            // Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null;
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var storedToken = await _refreshTokenRepo.GetQueryable()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.Expires <= DateTime.UtcNow)
            {
                return null;
            }

            var user = storedToken.User ?? await _userRepo.GetByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return null;
            }

            // Revoke the old refresh token (Token rotation)
            storedToken.IsRevoked = true;
            await _refreshTokenRepo.UpdateAsync(storedToken);
            await _refreshTokenRepo.SaveChangesAsync();

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepo.GetQueryable()
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (token == null || token.IsRevoked)
            {
                return false;
            }

            token.IsRevoked = true;
            await _refreshTokenRepo.UpdateAsync(token);
            await _refreshTokenRepo.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepo.GetByIdAsync(userId);
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            var secretKey = _configuration["JWT_SECRET"] 
                ?? _configuration["Jwt:SecretKey"] 
                ?? "PRN232_Advanced_REST_API_And_Security_JWT_Secret_Key_2026";
            var issuer = _configuration["Jwt:Issuer"] ?? "PRN232.LMS.API";
            var audience = _configuration["Jwt:Audience"] ?? "PRN232.LMS.Client";
            var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshTokenString = GenerateSecureRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.UserId,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepo.AddAsync(refreshTokenEntity);
            await _refreshTokenRepo.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresIn = expiryMinutes * 60,
                TokenType = "Bearer"
            };
        }

        private static string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }
    }
}

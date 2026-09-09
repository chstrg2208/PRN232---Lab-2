using System.Threading.Tasks;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Models.Requests;

namespace PRN232.LMS.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<User?> GetUserByIdAsync(int userId);
    }
}

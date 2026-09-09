using System.Threading.Tasks;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<PagedResult<object>> GetEnrollmentsAsync(QueryParameters queryParams);
        Task<object?> GetEnrollmentByIdAsync(int id, string? fields, string? expand);
        Task<EnrollmentResponse> CreateEnrollmentAsync(CreateEnrollmentRequest request);
        Task<EnrollmentResponse?> UpdateEnrollmentAsync(int id, UpdateEnrollmentRequest request);
        Task<bool> DeleteEnrollmentAsync(int id);
    }
}

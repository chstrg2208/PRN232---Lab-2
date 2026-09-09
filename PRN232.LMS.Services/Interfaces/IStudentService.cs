using System.Threading.Tasks;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResult<object>> GetStudentsAsync(QueryParameters queryParams);
        Task<object?> GetStudentByIdAsync(int id, string? fields, string? expand);
        Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request);
        Task<StudentResponse?> UpdateStudentAsync(int id, UpdateStudentRequest request);
        Task<bool> DeleteStudentAsync(int id);
    }
}

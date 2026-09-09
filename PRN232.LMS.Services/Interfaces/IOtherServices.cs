using System.Threading.Tasks;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<object>> GetCoursesAsync(QueryParameters queryParams);
        Task<object?> GetCourseByIdAsync(int id, string? fields, string? expand);
        Task<IEnumerable<StudentSummaryResponse>?> GetStudentsByCourseIdAsync(int courseId);
        Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request);
        Task<CourseResponse?> UpdateCourseAsync(int id, UpdateCourseRequest request);
        Task<bool> DeleteCourseAsync(int id);
    }

    public interface ISemesterService
    {
        Task<PagedResult<object>> GetSemestersAsync(QueryParameters queryParams);
        Task<object?> GetSemesterByIdAsync(int id, string? fields, string? expand);
        Task<SemesterResponse> CreateSemesterAsync(CreateSemesterRequest request);
        Task<SemesterResponse?> UpdateSemesterAsync(int id, UpdateSemesterRequest request);
        Task<bool> DeleteSemesterAsync(int id);
    }

    public interface ISubjectService
    {
        Task<PagedResult<object>> GetSubjectsAsync(QueryParameters queryParams);
        Task<object?> GetSubjectByIdAsync(int id, string? fields);
        Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request);
        Task<SubjectResponse?> UpdateSubjectAsync(int id, UpdateSubjectRequest request);
        Task<bool> DeleteSubjectAsync(int id);
    }
}

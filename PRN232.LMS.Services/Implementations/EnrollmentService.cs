using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Business;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IRepository<Enrollment> _enrollmentRepository;

        public EnrollmentService(IRepository<Enrollment> enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<PagedResult<object>> GetEnrollmentsAsync(QueryParameters queryParams)
        {
            var query = _enrollmentRepository.GetQueryable().AsNoTracking();

            // 1. Expansion
            bool expandStudent = !string.IsNullOrEmpty(queryParams.Expand) &&
                                 queryParams.Expand.Contains("student", StringComparison.OrdinalIgnoreCase);
            bool expandCourse = !string.IsNullOrEmpty(queryParams.Expand) &&
                                queryParams.Expand.Contains("course", StringComparison.OrdinalIgnoreCase);

            if (expandStudent) query = query.Include(e => e.Student);
            if (expandCourse) query = query.Include(e => e.Course);

            // 2. Searching
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
                query = query.Where(e => e.Status.ToLower().Contains(search) ||
                                         (e.Student != null && e.Student.FullName.ToLower().Contains(search)) ||
                                         (e.Course != null && e.Course.CourseName.ToLower().Contains(search)));
            }

            // 3. Sorting
            query = query.ApplySort(queryParams.Sort, defaultSort: "EnrollmentId");

            // 4. Paging
            var totalItems = await query.CountAsync();
            var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
            var pageSize = queryParams.Size <= 0 ? 10 : queryParams.Size;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var enrollments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Entity -> Business Model
            var businessModels = enrollments.Select(e => new EnrollmentBusinessModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                Student = e.Student != null ? new StudentBusinessModel
                {
                    StudentId = e.Student.StudentId,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email
                } : null,
                Course = e.Course != null ? new CourseBusinessModel
                {
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName
                } : null
            }).ToList();

            // 6. Business Model -> Response Model
            var responseModels = businessModels.Select(b => new EnrollmentResponse
            {
                EnrollmentId = b.EnrollmentId,
                StudentId = b.StudentId,
                CourseId = b.CourseId,
                EnrollDate = b.EnrollDate,
                Status = b.Status,
                Student = expandStudent && b.Student != null ? new StudentSummaryResponse
                {
                    StudentId = b.Student.StudentId,
                    FullName = b.Student.FullName,
                    Email = b.Student.Email
                } : null,
                Course = expandCourse && b.Course != null ? new CourseSummaryResponse
                {
                    CourseId = b.Course.CourseId,
                    CourseName = b.Course.CourseName
                } : null
            }).ToList();

            // 7. Field Selection / Shaping
            var shapedItems = DataShaper.ShapeCollection(responseModels, queryParams.Fields);

            return new PagedResult<object>
            {
                Items = shapedItems,
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<object?> GetEnrollmentByIdAsync(int id, string? fields, string? expand)
        {
            var query = _enrollmentRepository.GetQueryable().AsNoTracking();

            // Requirement: "Return complete related data for the resource. Avoid circular references and infinite recursion."
            query = query.Include(e => e.Student).Include(e => e.Course);

            var enrollment = await query.FirstOrDefaultAsync(e => e.EnrollmentId == id);
            if (enrollment == null) return null;

            // Entity -> Business Model
            var businessModel = new EnrollmentBusinessModel
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status,
                Student = enrollment.Student != null ? new StudentBusinessModel
                {
                    StudentId = enrollment.Student.StudentId,
                    FullName = enrollment.Student.FullName,
                    Email = enrollment.Student.Email
                } : null,
                Course = enrollment.Course != null ? new CourseBusinessModel
                {
                    CourseId = enrollment.Course.CourseId,
                    CourseName = enrollment.Course.CourseName
                } : null
            };

            // Business Model -> Response Model
            var response = new EnrollmentResponse
            {
                EnrollmentId = businessModel.EnrollmentId,
                StudentId = businessModel.StudentId,
                CourseId = businessModel.CourseId,
                EnrollDate = businessModel.EnrollDate,
                Status = businessModel.Status,
                Student = businessModel.Student != null ? new StudentSummaryResponse
                {
                    StudentId = businessModel.Student.StudentId,
                    FullName = businessModel.Student.FullName,
                    Email = businessModel.Student.Email
                } : null,
                Course = businessModel.Course != null ? new CourseSummaryResponse
                {
                    CourseId = businessModel.Course.CourseId,
                    CourseName = businessModel.Course.CourseName
                } : null
            };

            return DataShaper.ShapeObject(response, fields);
        }

        public async Task<EnrollmentResponse> CreateEnrollmentAsync(CreateEnrollmentRequest request)
        {
            // Request -> Business Model
            var businessModel = new EnrollmentBusinessModel
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                EnrollDate = DateTime.UtcNow,
                Status = request.Status
            };

            // Business Model -> Entity
            var entity = new Enrollment
            {
                StudentId = businessModel.StudentId,
                CourseId = businessModel.CourseId,
                EnrollDate = businessModel.EnrollDate,
                Status = businessModel.Status
            };

            await _enrollmentRepository.AddAsync(entity);
            await _enrollmentRepository.SaveChangesAsync();

            businessModel.EnrollmentId = entity.EnrollmentId;

            return new EnrollmentResponse
            {
                EnrollmentId = businessModel.EnrollmentId,
                StudentId = businessModel.StudentId,
                CourseId = businessModel.CourseId,
                EnrollDate = businessModel.EnrollDate,
                Status = businessModel.Status
            };
        }

        public async Task<EnrollmentResponse?> UpdateEnrollmentAsync(int id, UpdateEnrollmentRequest request)
        {
            var entity = await _enrollmentRepository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Status = request.Status;

            await _enrollmentRepository.UpdateAsync(entity);
            await _enrollmentRepository.SaveChangesAsync();

            return new EnrollmentResponse
            {
                EnrollmentId = entity.EnrollmentId,
                StudentId = entity.StudentId,
                CourseId = entity.CourseId,
                EnrollDate = entity.EnrollDate,
                Status = entity.Status
            };
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            var entity = await _enrollmentRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _enrollmentRepository.DeleteAsync(entity);
            await _enrollmentRepository.SaveChangesAsync();
            return true;
        }
    }
}

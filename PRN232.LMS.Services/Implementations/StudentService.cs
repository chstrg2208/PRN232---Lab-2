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
    public class StudentService : IStudentService
    {
        private readonly IRepository<Student> _studentRepository;

        public StudentService(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<PagedResult<object>> GetStudentsAsync(QueryParameters queryParams)
        {
            var query = _studentRepository.GetQueryable().AsNoTracking();

            // 1. Expansion
            bool expandEnrollments = !string.IsNullOrEmpty(queryParams.Expand) &&
                                     queryParams.Expand.Contains("enrollment", StringComparison.OrdinalIgnoreCase);

            if (expandEnrollments)
            {
                query = query.Include(s => s.Enrollments).ThenInclude(e => e.Course);
            }

            // 2. Searching
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
                query = query.Where(s => s.FullName.ToLower().Contains(search) || s.Email.ToLower().Contains(search));
            }

            // 3. Sorting
            query = query.ApplySort(queryParams.Sort, defaultSort: "StudentId");

            // 4. Paging
            var totalItems = await query.CountAsync();
            var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
            var pageSize = queryParams.Size <= 0 ? 10 : queryParams.Size;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var students = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Entity -> Business Model
            var businessModels = students.Select(s => new StudentBusinessModel
            {
                StudentId = s.StudentId,
                FullName = s.FullName,
                Email = s.Email,
                DateOfBirth = s.DateOfBirth,
                RollNumber = s.RollNumber,
                PhoneNumber = s.PhoneNumber,
                Enrollments = s.Enrollments.Select(e => new EnrollmentBusinessModel
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    Course = e.Course != null ? new CourseBusinessModel
                    {
                        CourseId = e.Course.CourseId,
                        CourseName = e.Course.CourseName
                    } : null,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList()
            }).ToList();

            // 6. Business Model -> Response Model
            var responseModels = businessModels.Select(b => new StudentResponse
            {
                StudentId = b.StudentId,
                FullName = b.FullName,
                Email = b.Email,
                DateOfBirth = b.DateOfBirth,
                RollNumber = b.RollNumber,
                PhoneNumber = b.PhoneNumber,
                Enrollments = expandEnrollments ? b.Enrollments.Select(e => new EnrollmentSummaryResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseName = e.Course?.CourseName,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList() : null
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

        public async Task<object?> GetStudentByIdAsync(int id, string? fields, string? expand)
        {
            var query = _studentRepository.GetQueryable().AsNoTracking();

            // Requirement: "Return complete related data for the resource. Avoid circular references and infinite recursion."
            query = query.Include(s => s.Enrollments).ThenInclude(e => e.Course);

            var student = await query.FirstOrDefaultAsync(s => s.StudentId == id);
            if (student == null) return null;

            // Entity -> Business Model
            var businessModel = new StudentBusinessModel
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                RollNumber = student.RollNumber,
                PhoneNumber = student.PhoneNumber,
                Enrollments = student.Enrollments.Select(e => new EnrollmentBusinessModel
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    Course = e.Course != null ? new CourseBusinessModel
                    {
                        CourseId = e.Course.CourseId,
                        CourseName = e.Course.CourseName
                    } : null,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList()
            };

            // Business Model -> Response Model
            var response = new StudentResponse
            {
                StudentId = businessModel.StudentId,
                FullName = businessModel.FullName,
                Email = businessModel.Email,
                DateOfBirth = businessModel.DateOfBirth,
                RollNumber = businessModel.RollNumber,
                PhoneNumber = businessModel.PhoneNumber,
                Enrollments = businessModel.Enrollments.Select(e => new EnrollmentSummaryResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseName = e.Course?.CourseName,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList()
            };

            return DataShaper.ShapeObject(response, fields);
        }

        public async Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request)
        {
            // Request -> Business Model
            var businessModel = new StudentBusinessModel
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
                RollNumber = request.RollNumber,
                PhoneNumber = request.PhoneNumber
            };

            // Business Model -> Entity
            var entity = new Student
            {
                FullName = businessModel.FullName,
                Email = businessModel.Email,
                DateOfBirth = businessModel.DateOfBirth,
                RollNumber = businessModel.RollNumber,
                PhoneNumber = businessModel.PhoneNumber
            };

            await _studentRepository.AddAsync(entity);
            await _studentRepository.SaveChangesAsync();

            businessModel.StudentId = entity.StudentId;

            return new StudentResponse
            {
                StudentId = businessModel.StudentId,
                FullName = businessModel.FullName,
                Email = businessModel.Email,
                DateOfBirth = businessModel.DateOfBirth,
                RollNumber = businessModel.RollNumber,
                PhoneNumber = businessModel.PhoneNumber
            };
        }

        public async Task<StudentResponse?> UpdateStudentAsync(int id, UpdateStudentRequest request)
        {
            var entity = await _studentRepository.GetByIdAsync(id);
            if (entity == null) return null;

            // Request -> Business Model
            var businessModel = new StudentBusinessModel
            {
                StudentId = id,
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
                RollNumber = request.RollNumber,
                PhoneNumber = request.PhoneNumber
            };

            entity.FullName = businessModel.FullName;
            entity.Email = businessModel.Email;
            entity.DateOfBirth = businessModel.DateOfBirth;
            entity.RollNumber = businessModel.RollNumber;
            entity.PhoneNumber = businessModel.PhoneNumber;

            await _studentRepository.UpdateAsync(entity);
            await _studentRepository.SaveChangesAsync();

            return new StudentResponse
            {
                StudentId = businessModel.StudentId,
                FullName = businessModel.FullName,
                Email = businessModel.Email,
                DateOfBirth = businessModel.DateOfBirth,
                RollNumber = businessModel.RollNumber,
                PhoneNumber = businessModel.PhoneNumber
            };
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var entity = await _studentRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _studentRepository.DeleteAsync(entity);
            await _studentRepository.SaveChangesAsync();
            return true;
        }
    }
}

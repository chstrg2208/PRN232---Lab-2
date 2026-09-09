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
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course> _courseRepo;

        public CourseService(IRepository<Course> courseRepo)
        {
            _courseRepo = courseRepo;
        }

        public async Task<PagedResult<object>> GetCoursesAsync(QueryParameters queryParams)
        {
            var query = _courseRepo.GetQueryable().AsNoTracking();

            bool expandSemester = !string.IsNullOrEmpty(queryParams.Expand) &&
                                  queryParams.Expand.Contains("semester", StringComparison.OrdinalIgnoreCase);

            if (expandSemester) query = query.Include(c => c.Semester);

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
                query = query.Where(c => c.CourseName.ToLower().Contains(search));
            }

            query = query.ApplySort(queryParams.Sort, defaultSort: "CourseId");

            var totalItems = await query.CountAsync();
            var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
            var pageSize = queryParams.Size <= 0 ? 10 : queryParams.Size;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var courses = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var responseModels = courses.Select(c => new CourseResponse
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                Semester = expandSemester && c.Semester != null ? new SemesterSummaryResponse
                {
                    SemesterId = c.Semester.SemesterId,
                    SemesterName = c.Semester.SemesterName
                } : null
            }).ToList();

            var shaped = DataShaper.ShapeCollection(responseModels, queryParams.Fields);

            return new PagedResult<object>
            {
                Items = shaped,
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<object?> GetCourseByIdAsync(int id, string? fields, string? expand)
        {
            var query = _courseRepo.GetQueryable().AsNoTracking()
                .Include(c => c.Semester)
                .Include(c => c.Enrollments);

            var course = await query.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return null;

            var response = new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId,
                Semester = course.Semester != null ? new SemesterSummaryResponse
                {
                    SemesterId = course.Semester.SemesterId,
                    SemesterName = course.Semester.SemesterName
                } : null,
                Enrollments = course.Enrollments.Select(e => new EnrollmentSummaryResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList()
            };

            return DataShaper.ShapeObject(response, fields);
        }

        public async Task<IEnumerable<StudentSummaryResponse>?> GetStudentsByCourseIdAsync(int courseId)
        {
            var course = await _courseRepo.GetQueryable()
                .AsNoTracking()
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return null;

            return course.Enrollments
                .Where(e => e.Student != null)
                .Select(e => new StudentSummaryResponse
                {
                    StudentId = e.Student!.StudentId,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,
                    RollNumber = e.Student.RollNumber
                }).ToList();
        }

        public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request)
        {
            var entity = new Course
            {
                CourseName = request.CourseName,
                SemesterId = request.SemesterId
            };

            await _courseRepo.AddAsync(entity);
            await _courseRepo.SaveChangesAsync();

            return new CourseResponse
            {
                CourseId = entity.CourseId,
                CourseName = entity.CourseName,
                SemesterId = entity.SemesterId
            };
        }

        public async Task<CourseResponse?> UpdateCourseAsync(int id, UpdateCourseRequest request)
        {
            var entity = await _courseRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.CourseName = request.CourseName;
            entity.SemesterId = request.SemesterId;

            await _courseRepo.UpdateAsync(entity);
            await _courseRepo.SaveChangesAsync();

            return new CourseResponse
            {
                CourseId = entity.CourseId,
                CourseName = entity.CourseName,
                SemesterId = entity.SemesterId
            };
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var entity = await _courseRepo.GetByIdAsync(id);
            if (entity == null) return false;

            await _courseRepo.DeleteAsync(entity);
            await _courseRepo.SaveChangesAsync();
            return true;
        }
    }

    public class SemesterService : ISemesterService
    {
        private readonly IRepository<Semester> _semesterRepo;

        public SemesterService(IRepository<Semester> semesterRepo)
        {
            _semesterRepo = semesterRepo;
        }

        public async Task<PagedResult<object>> GetSemestersAsync(QueryParameters queryParams)
        {
            var query = _semesterRepo.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
                query = query.Where(s => s.SemesterName.ToLower().Contains(search));
            }

            query = query.ApplySort(queryParams.Sort, defaultSort: "SemesterId");

            var totalItems = await query.CountAsync();
            var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
            var pageSize = queryParams.Size <= 0 ? 10 : queryParams.Size;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var responseModels = list.Select(s => new SemesterResponse
            {
                SemesterId = s.SemesterId,
                SemesterName = s.SemesterName,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            }).ToList();

            var shaped = DataShaper.ShapeCollection(responseModels, queryParams.Fields);

            return new PagedResult<object>
            {
                Items = shaped,
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<object?> GetSemesterByIdAsync(int id, string? fields, string? expand)
        {
            var query = _semesterRepo.GetQueryable().AsNoTracking().Include(s => s.Courses);
            var semester = await query.FirstOrDefaultAsync(s => s.SemesterId == id);
            if (semester == null) return null;

            var response = new SemesterResponse
            {
                SemesterId = semester.SemesterId,
                SemesterName = semester.SemesterName,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate,
                Courses = semester.Courses.Select(c => new CourseSummaryResponse
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName
                }).ToList()
            };

            return DataShaper.ShapeObject(response, fields);
        }

        public async Task<SemesterResponse> CreateSemesterAsync(CreateSemesterRequest request)
        {
            var entity = new Semester
            {
                SemesterName = request.SemesterName,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _semesterRepo.AddAsync(entity);
            await _semesterRepo.SaveChangesAsync();

            return new SemesterResponse
            {
                SemesterId = entity.SemesterId,
                SemesterName = entity.SemesterName,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };
        }

        public async Task<SemesterResponse?> UpdateSemesterAsync(int id, UpdateSemesterRequest request)
        {
            var entity = await _semesterRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.SemesterName = request.SemesterName;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;

            await _semesterRepo.UpdateAsync(entity);
            await _semesterRepo.SaveChangesAsync();

            return new SemesterResponse
            {
                SemesterId = entity.SemesterId,
                SemesterName = entity.SemesterName,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };
        }

        public async Task<bool> DeleteSemesterAsync(int id)
        {
            var entity = await _semesterRepo.GetByIdAsync(id);
            if (entity == null) return false;

            await _semesterRepo.DeleteAsync(entity);
            await _semesterRepo.SaveChangesAsync();
            return true;
        }
    }

    public class SubjectService : ISubjectService
    {
        private readonly IRepository<Subject> _subjectRepo;

        public SubjectService(IRepository<Subject> subjectRepo)
        {
            _subjectRepo = subjectRepo;
        }

        public async Task<PagedResult<object>> GetSubjectsAsync(QueryParameters queryParams)
        {
            var query = _subjectRepo.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
                query = query.Where(s => s.SubjectName.ToLower().Contains(search) || s.SubjectCode.ToLower().Contains(search));
            }

            query = query.ApplySort(queryParams.Sort, defaultSort: "SubjectId");

            var totalItems = await query.CountAsync();
            var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
            var pageSize = queryParams.Size <= 0 ? 10 : queryParams.Size;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var responseModels = list.Select(s => new SubjectResponse
            {
                SubjectId = s.SubjectId,
                SubjectCode = s.SubjectCode,
                SubjectName = s.SubjectName,
                Credit = s.Credit
            }).ToList();

            var shaped = DataShaper.ShapeCollection(responseModels, queryParams.Fields);

            return new PagedResult<object>
            {
                Items = shaped,
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<object?> GetSubjectByIdAsync(int id, string? fields)
        {
            var subject = await _subjectRepo.GetByIdAsync(id);
            if (subject == null) return null;

            var response = new SubjectResponse
            {
                SubjectId = subject.SubjectId,
                SubjectCode = subject.SubjectCode,
                SubjectName = subject.SubjectName,
                Credit = subject.Credit
            };

            return DataShaper.ShapeObject(response, fields);
        }

        public async Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request)
        {
            var entity = new Subject
            {
                SubjectCode = request.SubjectCode,
                SubjectName = request.SubjectName,
                Credit = request.Credit
            };

            await _subjectRepo.AddAsync(entity);
            await _subjectRepo.SaveChangesAsync();

            return new SubjectResponse
            {
                SubjectId = entity.SubjectId,
                SubjectCode = entity.SubjectCode,
                SubjectName = entity.SubjectName,
                Credit = entity.Credit
            };
        }

        public async Task<SubjectResponse?> UpdateSubjectAsync(int id, UpdateSubjectRequest request)
        {
            var entity = await _subjectRepo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.SubjectCode = request.SubjectCode;
            entity.SubjectName = request.SubjectName;
            entity.Credit = request.Credit;

            await _subjectRepo.UpdateAsync(entity);
            await _subjectRepo.SaveChangesAsync();

            return new SubjectResponse
            {
                SubjectId = entity.SubjectId,
                SubjectCode = entity.SubjectCode,
                SubjectName = entity.SubjectName,
                Credit = entity.Credit
            };
        }

        public async Task<bool> DeleteSubjectAsync(int id)
        {
            var entity = await _subjectRepo.GetByIdAsync(id);
            if (entity == null) return false;

            await _subjectRepo.DeleteAsync(entity);
            await _subjectRepo.SaveChangesAsync();
            return true;
        }
    }
}

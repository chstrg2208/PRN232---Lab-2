using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using Bogus;
using System;
using System.Collections.Generic;

namespace PRN232.LMS.Repositories.Data
{
    public class LmsDbContext : DbContext
    {
        public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options)
        {
        }

        public DbSet<Semester> Semesters { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Semester>(entity =>
            {
                entity.HasKey(e => e.SemesterId);
                entity.Property(e => e.SemesterName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.Property(e => e.CourseName).HasMaxLength(100).IsRequired();
                entity.HasOne(e => e.Semester)
                      .WithMany(s => s.Courses)
                      .HasForeignKey(e => e.SemesterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.SubjectId);
                entity.Property(e => e.SubjectCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.SubjectName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Credit).IsRequired();
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.StudentId);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.RollNumber).HasMaxLength(20);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId);
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.EnrollDate).IsRequired();

                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.RefreshTokenId);
                entity.Property(e => e.Token).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Expires).IsRequired();
                entity.Property(e => e.Created).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            Randomizer.Seed = new Random(8675309);

            // 1. Semesters (5)
            var semesters = new List<Semester>();
            var baseDate = new DateTime(2023, 1, 1);
            for (int i = 1; i <= 5; i++)
            {
                semesters.Add(new Semester
                {
                    SemesterId = i,
                    SemesterName = $"Semester {i}",
                    StartDate = baseDate.AddMonths((i - 1) * 4),
                    EndDate = baseDate.AddMonths(i * 4).AddDays(-1)
                });
            }
            modelBuilder.Entity<Semester>().HasData(semesters);

            // 2. Subjects (10)
            var subjects = new List<Subject>
            {
                new() { SubjectId = 1, SubjectCode = "PRN232", SubjectName = "Advanced .NET Programming", Credit = 3 },
                new() { SubjectId = 2, SubjectCode = "PRN211", SubjectName = "Basic .NET Programming", Credit = 3 },
                new() { SubjectId = 3, SubjectCode = "SWP391", SubjectName = "Software Development Project", Credit = 4 },
                new() { SubjectId = 4, SubjectCode = "PRJ301", SubjectName = "Java Web Application", Credit = 3 },
                new() { SubjectId = 5, SubjectCode = "DBI202", SubjectName = "Introduction to Databases", Credit = 3 },
                new() { SubjectId = 6, SubjectCode = "MAS291", SubjectName = "Statistics for IT", Credit = 3 },
                new() { SubjectId = 7, SubjectCode = "SWE201", SubjectName = "Software Engineering", Credit = 3 },
                new() { SubjectId = 8, SubjectCode = "PRM392", SubjectName = "Mobile Programming", Credit = 3 },
                new() { SubjectId = 9, SubjectCode = "IOT102", SubjectName = "Internet of Things", Credit = 3 },
                new() { SubjectId = 10, SubjectCode = "EXE101", SubjectName = "Experiential Entrepreneurship", Credit = 3 }
            };
            modelBuilder.Entity<Subject>().HasData(subjects);

            // 3. Students (50)
            var studentId = 1;
            var studentFaker = new Faker<Student>()
                .RuleFor(s => s.StudentId, f => studentId++)
                .RuleFor(s => s.FullName, f => f.Name.FullName())
                .RuleFor(s => s.Email, (f, s) => $"student{s.StudentId}@fpt.edu.vn")
                .RuleFor(s => s.DateOfBirth, f => f.Date.Between(new DateTime(2000, 1, 1), new DateTime(2005, 12, 31)));
            var students = studentFaker.Generate(50);
            modelBuilder.Entity<Student>().HasData(students);

            // 4. Courses (20)
            var courseId = 1;
            var courseNames = new[]
            {
                "PRN232 - Class SE1801", "PRN232 - Class SE1802", "PRN211 - Class SE1701", "SWP391 - Project Group 1",
                "PRJ301 - Class SE1601", "DBI202 - Class SE1501", "MAS291 - Class MA01", "SWE201 - Class SE1803",
                "PRM392 - Class SE1804", "IOT102 - Class IA01", "PRN232 - Class IA02", "PRJ301 - Class IA03",
                "SWP391 - Project Group 2", "DBI202 - Class IA04", "SWE201 - Class IA05", "PRM392 - Class IA06",
                "PRN211 - Class IA07", "EXE101 - Class BA01", "EXE101 - Class BA02", "MAS291 - Class MA02"
            };
            var courses = new List<Course>();
            for (int i = 0; i < 20; i++)
            {
                courses.Add(new Course
                {
                    CourseId = courseId++,
                    CourseName = courseNames[i],
                    SemesterId = (i % 5) + 1
                });
            }
            modelBuilder.Entity<Course>().HasData(courses);

            // 5. Enrollments (500)
            var enrollmentId = 1;
            var statuses = new[] { "Active", "Completed", "Dropped" };
            var enrollments = new List<Enrollment>();
            var random = new Random(8675309);

            for (int i = 1; i <= 500; i++)
            {
                enrollments.Add(new Enrollment
                {
                    EnrollmentId = enrollmentId++,
                    StudentId = (i % 50) + 1,
                    CourseId = (i % 20) + 1,
                    EnrollDate = baseDate.AddDays(random.Next(1, 400)),
                    Status = statuses[random.Next(statuses.Length)]
                });
            }
            modelBuilder.Entity<Enrollment>().HasData(enrollments);

            // 6. Users (Admin and Student)
            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 10);
            var users = new List<User>
            {
                new()
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = defaultPasswordHash,
                    Role = "Admin"
                },
                new()
                {
                    UserId = 2,
                    Username = "student",
                    PasswordHash = defaultPasswordHash,
                    Role = "Student"
                }
            };
            modelBuilder.Entity<User>().HasData(users);
        }
    }
}

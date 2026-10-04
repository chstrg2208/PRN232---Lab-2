using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRN232.LMS.API.Middlewares;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & Content Negotiation (JSON + XML + HTTP 406 for unsupported)
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlDataContractSerializerFormatters()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// 2. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentRequestValidator>();

// 3. Database Context
builder.Services.AddDbContext<LmsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 4. Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 5. Register Services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 6. API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// 7. JWT Authentication & Authorization
var jwtSecretKey = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["Jwt:SecretKey"]
    ?? "PRN232_Advanced_REST_API_And_Security_JWT_Secret_Key_2026";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PRN232.LMS.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PRN232.LMS.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 8. Swagger / OpenAPI Documentation with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS API - v1",
        Version = "v1",
        Description = "ASP.NET Core RESTful API v1 with 3-layer architecture, Content Negotiation, and JWT Security."
    });

    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "PRN232 LMS API - v2",
        Version = "v2",
        Description = "ASP.NET Core RESTful API v2 with advanced features."
    });

    // JWT Bearer configuration in Swagger (Authorize button)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only (no need to prefix with 'Bearer ')"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 9. Custom Middlewares
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// 10. Auto Database Creation & Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<LmsDbContext>();

    int retries = 10;
    while (retries > 0)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database and ensure created/seeded...");
            dbContext.Database.EnsureCreated();

            if (!dbContext.Users.Any())
            {
                logger.LogInformation("Seeding default Lab 2 users (admin, student)...");
                var hash = BCrypt.Net.BCrypt.HashPassword("123456", 10);
                dbContext.Users.AddRange(
                    new PRN232.LMS.Repositories.Entities.User
                    {
                        Username = "admin",
                        PasswordHash = hash,
                        Role = "Admin"
                    },
                    new PRN232.LMS.Repositories.Entities.User
                    {
                        Username = "student",
                        PasswordHash = hash,
                        Role = "Student"
                    }
                );
                dbContext.SaveChanges();
                logger.LogInformation("Default users seeded successfully.");
            }

            logger.LogInformation("Database ready and seeded successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning("Database connection failed. Retrying in 4 seconds... ({Remaining} retries left). Error: {Message}", retries, ex.Message);
            if (retries == 0)
            {
                logger.LogError(ex, "Could not connect to database after several attempts.");
            }
            else
            {
                Thread.Sleep(4000);
            }
        }
    }
}

// 11. Swagger UI with Multi-Version Support
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "PRN232 LMS API v2");
    c.RoutePrefix = string.Empty; // Access Swagger UI directly at http://localhost:<port>/
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Allow accessing Swagger via both / and /swagger
app.MapGet("/swagger", () => Results.Redirect("/index.html"));
app.MapGet("/swagger/index.html", () => Results.Redirect("/index.html"));

app.Run();
